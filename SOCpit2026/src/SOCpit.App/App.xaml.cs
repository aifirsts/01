// SOCpit - Security Operations Center
// Application entry point
// Based on MATRIX V3 with WPF

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SOCpit.App.ViewModels;
using SOCpit.Application.Repositories;
using SOCpit.Application.Services;
using SOCpit.Core;
using SOCpit.Import.Exporters;
using SOCpit.Import.Importers;
using SOCpit.Storage;
using SOCpit.Storage.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace SOCpit.App
{
    /// <summary>Interaction logic for App.xaml</summary>
    public partial class App : System.Windows.Application
    {
        public static IServiceProvider? ServiceProvider { get; private set; }
        public static string AppDataPath { get; private set; } = ResolveDataPath();
        public static string DatabasePath => Path.Combine(AppDataPath, "socpit.db");

        private static string ResolveDataPath()
        {
            const string preferredWindowsPath = @"C:\SOCpit2026";
            try
            {
                if (OperatingSystem.IsWindows())
                {
                    Directory.CreateDirectory(preferredWindowsPath);
                    return preferredWindowsPath;
                }
            }
            catch
            {
                // Fall back to LocalApplicationData if C:\SOCpit2026 is not writable
            }

            var fallback = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SOCpit2026");
            Directory.CreateDirectory(fallback);
            return fallback;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Directory.CreateDirectory(AppDataPath);

            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            InitializeDatabase();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            MainWindow = mainWindow;
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(logging =>
            {
                logging.AddDebug();
                logging.AddConsole();
            });

            services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
                .SetBasePath(AppDataPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build());

            services.AddDbContext<SOCpitDbContext>(options =>
            {
                options.UseSqlite("Data Source=" + DatabasePath);
            }, ServiceLifetime.Transient);

            services.AddTransient<IThreatRepository, ThreatRepository>();
            services.AddTransient<IVulnerabilityRepository, VulnerabilityRepository>();
            services.AddTransient<IIncidentRepository, IncidentRepository>();
            services.AddTransient<IEvidenceRepository, EvidenceRepository>();
            services.AddTransient<IHostRepository, HostRepository>();
            services.AddTransient<INetworkRepository, NetworkRepository>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IAuditLogRepository, AuditLogRepository>();
            services.AddTransient<IUnitOfWork, UnitOfWork>();

            services.AddTransient<ThreatService>();
            services.AddTransient<VulnerabilityService>();
            services.AddTransient<IncidentService>();
            services.AddTransient<EvidenceService>();
            services.AddTransient<HostService>();
            services.AddTransient<NetworkService>();
            services.AddTransient<UserService>();
            services.AddTransient<AuditService>();

            services.AddTransient<MatrixImporter>();
            services.AddTransient<MatrixExporter>();

            services.AddSingleton<MainViewModel>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<ThreatsViewModel>();
            services.AddTransient<VulnerabilitiesViewModel>();
            services.AddTransient<IncidentsViewModel>();
            services.AddTransient<EvidenceViewModel>();
            services.AddTransient<HostsViewModel>();
            services.AddTransient<NetworksViewModel>();
            services.AddTransient<UsersViewModel>();
            services.AddTransient<ReportsViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddSingleton<MainWindow>();
        }

        private void InitializeDatabase()
        {
            try
            {
                using var scope = ServiceProvider!.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<SOCpitDbContext>();
                dbContext.Database.EnsureCreated();

                if (!dbContext.Hosts.Any())
                {
                    SeedInitialData(dbContext);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database initialization failed: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void SeedInitialData(SOCpitDbContext dbContext)
        {
            var now = DateTimeOffset.UtcNow;

            dbContext.Hosts.AddRange(
                Host.Create("host-neo-pc", "NEO Workstation", "192.168.1.10", "00:1A:2B:3C:4D:5E",
                    AssetType.Workstation, AssetStatus.Active, OperatingSystemType.Windows,
                    now.AddDays(-30), new List<string> { "neo", "workstation", "core" },
                    "Windows 11 Pro", "NEO-PC", "WORKGROUP", "Home Office", "NEO", now),
                Host.Create("host-vps-01", "Production VPS Gateway", "185.199.108.15", "00:1A:2B:3C:4D:5F",
                    AssetType.Server, AssetStatus.Active, OperatingSystemType.Linux,
                    now.AddDays(-60), new List<string> { "vps", "edge", "production" },
                    "Ubuntu 24.04 LTS", "vps-edge-01", "neo.infra", "Cloud EU", "NEO", now),
                Host.Create("host-router-01", "Core Home Router", "192.168.1.1", "00:1A:2B:3C:4D:60",
                    AssetType.NetworkDevice, AssetStatus.Active, OperatingSystemType.Linux,
                    now.AddDays(-90), new List<string> { "router", "firewall", "lan" },
                    "RouterOS 7", "gw-router", "local", "Home Office", "NEO", now)
            );

            dbContext.Networks.AddRange(
                Network.Create("net-lan-01", "NEO Internal LAN", "192.168.1.0/24",
                    "Primary internal trust zone network for NEO workstation and local devices",
                    "Internal", new List<string> { "internal", "trusted" },
                    new List<string> { "vlan-10" }, new List<string> { "subnet-lan-1" }),
                Network.Create("net-dmz-01", "Cloud VPS Perimeter", "10.10.0.0/24",
                    "External VPS and public service perimeter network",
                    "DMZ", new List<string> { "dmz", "vps" },
                    new List<string> { "vlan-20" }, new List<string> { "subnet-dmz-1" })
            );

            dbContext.Threats.AddRange(
                Threat.Create("threat-001", "SSH Brute-Force Attempt on VPS",
                    "Multiple failed root SSH authentication attempts detected from external IP range.",
                    ThreatSeverity.High, ThreatStatus.Investigating, "host-vps-01",
                    now.AddHours(-6), "MATRIX V3 / Fail2ban",
                    new List<string> { "ssh", "brute-force", "vps" },
                    "Enforce key-only SSH and block offending CIDR via firewall", "evidence-001"),
                Threat.Create("threat-002", "Unauthorized Port Scan Detected",
                    "SYN scan across common management ports detected on perimeter interface.",
                    ThreatSeverity.Medium, ThreatStatus.Contained, "host-router-01",
                    now.AddHours(-18), "Firewall IDS",
                    new List<string> { "reconnaissance", "port-scan" },
                    "Drop unsolicited ingress packets on WAN interface", null),
                Threat.Create("threat-003", "Suspicious PowerShell Execution Policy Change",
                    "Audit alert for script execution policy inspection on workstation.",
                    ThreatSeverity.Critical, ThreatStatus.New, "host-neo-pc",
                    now.AddHours(-2), "Windows Defender / EDR",
                    new List<string> { "powershell", "workstation", "edr" },
                    "Verify operator command history and enforce AllSigned policy", "evidence-002")
            );

            dbContext.Vulnerabilities.AddRange(
                Vulnerability.Create("vuln-001", "CVE-2026-1042", "OpenSSH Pre-Auth Race Condition",
                    "Potential remote code execution or authentication bypass in unpatched sshd daemon.",
                    VulnerabilitySeverity.Critical, VulnerabilityStatus.Open, "host-vps-01",
                    now.AddDays(-5), "Upgrade openssh-server to >= 9.9p2", "9.1", now,
                    new List<string> { "https://nvd.nist.gov/vuln/detail/CVE-2026-1042" }),
                Vulnerability.Create("vuln-002", "CVE-2026-0819", "Router Webfig TLS Cipher Weakness",
                    "Deprecated TLS cipher suites enabled on management interface.",
                    VulnerabilitySeverity.Medium, VulnerabilityStatus.InProgress, "host-router-01",
                    now.AddDays(-12), "Disable TLS 1.0/1.1 and weak ciphers", "5.8", now,
                    new List<string> { "https://nvd.nist.gov/vuln/detail/CVE-2026-0819" })
            );

            dbContext.SecurityIncidents.AddRange(
                SecurityIncident.Create("incident-001", "Perimeter VPS SSH Intrusion Attempt",
                    "Coordinated credential stuffing and port scanning against Production VPS Gateway.",
                    IncidentSeverity.High, IncidentStatus.Investigating,
                    now.AddHours(-5), "NEO",
                    new List<string> { "threat-001", "threat-002" },
                    new List<string> { "vuln-001" },
                    "No unauthorized login succeeded; SSH key-only policy prevented compromise.",
                    "Blocked source IPs, rotated SSH host keys, scheduled OpenSSH package upgrade.")
            );

            dbContext.Evidence.AddRange(
                Evidence.Create("evidence-001", "VPS Auth.log Capture",
                    EvidenceType.Log, "Captured /var/log/auth.log segment showing blocked SSH attempts",
                    EvidenceStatus.Analyzed, EvidenceClassification.Confidential,
                    "host-vps-01", now.AddHours(-5), 4096,
                    "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
                    @"C:\SOCpit2026\evidence\vps-auth.log",
                    new List<string> { "ssh", "auth-log" },
                    "incident-001", "threat-001", "vuln-001",
                    "Verified all 47 attempts were rejected by pubkey enforcement.", "NEO"),
                Evidence.Create("evidence-002", "Workstation Security Event Log",
                    EvidenceType.AuditLog, "Windows Security Event ID 4688 & PowerShell 4104 export",
                    EvidenceStatus.Reviewed, EvidenceClassification.Internal,
                    "host-neo-pc", now.AddHours(-1), 8192,
                    "a591a6d40bf420404a011733cfb7b190d62c65bf0bcda32b57b277d9ad9f146e",
                    @"C:\SOCpit2026\evidence\neo-pc-events.evtx",
                    new List<string> { "windows", "eventlog" },
                    null, "threat-003", null,
                    "Confirmed administrative script run by NEO during maintenance.", "NEO")
            );

            dbContext.Users.AddRange(
                User.Create("user-neo", "NEO", "neo@socpit.local", "NEO (Lead Security Operator)",
                    UserRole.SuperAdministrator, UserStatus.Active, AuthenticationType.MultiFactor,
                    now.AddDays(-90), new List<string> { "owner", "admin", "soc" },
                    new List<string> { "perm-all" }, "Security Operations", "+7-000-000-0000", now),
                User.Create("user-analyst", "analyst", "analyst@socpit.local", "SOC Tier-2 Analyst",
                    UserRole.Analyst, UserStatus.Active, AuthenticationType.Local,
                    now.AddDays(-30), new List<string> { "analyst", "soc" },
                    new List<string> { "perm-read", "perm-triage" }, "Threat Intelligence", null, now.AddHours(-4))
            );

            dbContext.AuditLogs.AddRange(
                AuditLog.Create("audit-001", "Initialize", "Workspace", "socpit-2026", "NEO",
                    now.AddMinutes(-30), "Initialized SOCpit v2026.10.11 database with ADR-V2-SEC-001 policy"),
                AuditLog.Create("audit-002", "Import", "MATRIX", "matrix-v3", "NEO",
                    now.AddMinutes(-15), "Synchronized infrastructure assets and security controls from MATRIX V3")
            );

            dbContext.SaveChanges();
        }
    }
}
