// SOCpit - Security Operations Center
// Infrastructure domain models for SOCpit project
// Based on MATRIX V3 with infrastructure management

using System;
using System.Collections.Generic;

namespace SOCpit.Core
{
    /// <summary>Asset types.</summary>
    public enum AssetType
    {
        Server,
        Workstation,
        NetworkDevice,
        Application,
        Database,
        CloudResource,
        IoTDevice,
        MobileDevice,
        Other
    }

    /// <summary>Asset status.</summary>
    public enum AssetStatus
    {
        Active,
        Inactive,
        Retired,
        Maintenance,
        Unknown
    }

    /// <summary>Operating system types.</summary>
    public enum OperatingSystemType
    {
        Windows,
        Linux,
        macOS,
        Unix,
        iOS,
        Android,
        Other
    }

    /// <summary>Network protocol types.</summary>
    public enum NetworkProtocol
    {
        TCP,
        UDP,
        ICMP,
        HTTP,
        HTTPS,
        FTP,
        SSH,
        DNS,
        DHCP,
        SMTP,
        Other
    }

    /// <summary>A network connection.</summary>
    public sealed class NetworkConnection
    {
        public required string Id { get; init; }
        public required string SourceIp { get; init; }
        public required int SourcePort { get; init; }
        public required string DestinationIp { get; init; }
        public required int DestinationPort { get; init; }
        public required NetworkProtocol Protocol { get; init; }
        public required DateTimeOffset EstablishedAt { get; init; }
        public required DateTimeOffset? TerminatedAt { get; init; }
        public required long BytesSent { get; init; }
        public required long BytesReceived { get; init; }
        public required string? ProcessName { get; init; }
        public required string? UserId { get; init; }

        public static NetworkConnection Create(
            string id, string sourceIp, int sourcePort, string destinationIp,
            int destinationPort, NetworkProtocol protocol,
            DateTimeOffset establishedAt, long bytesSent = 0, long bytesReceived = 0,
            string? processName = null, string? userId = null,
            DateTimeOffset? terminatedAt = null)
        {
            return new NetworkConnection
            {
                Id = Threat.ValidateId(id),
                SourceIp = sourceIp ?? throw new ArgumentException("sourceIp"),
                SourcePort = sourcePort >= 0 && sourcePort <= 65535 ? sourcePort : throw new ArgumentException("sourcePort"),
                DestinationIp = destinationIp ?? throw new ArgumentException("destinationIp"),
                DestinationPort = destinationPort >= 0 && destinationPort <= 65535 ? destinationPort : throw new ArgumentException("destinationPort"),
                Protocol = protocol,
                EstablishedAt = establishedAt,
                TerminatedAt = terminatedAt,
                BytesSent = bytesSent >= 0 ? bytesSent : 0,
                BytesReceived = bytesReceived >= 0 ? bytesReceived : 0,
                ProcessName = processName,
                UserId = userId
            };
        }
    }

    /// <summary>A host asset.</summary>
    public sealed class Host
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string IpAddress { get; init; }
        public required string MacAddress { get; init; }
        public required AssetType Type { get; init; }
        public required AssetStatus Status { get; init; }
        public required OperatingSystemType OperatingSystem { get; init; }
        public required string? OsVersion { get; init; }
        public required string? Hostname { get; init; }
        public required string? Domain { get; init; }
        public required string? Location { get; init; }
        public required IReadOnlyList<string> Tags { get; init; }
        public required string? Owner { get; init; }
        public required DateTimeOffset? LastSeenAt { get; init; }
        public required DateTimeOffset CreatedAt { get; init; }

        public static Host Create(
            string id, string name, string ipAddress, string macAddress,
            AssetType type, AssetStatus status, OperatingSystemType operatingSystem,
            DateTimeOffset createdAt, IReadOnlyList<string> tags,
            string? osVersion = null, string? hostname = null, string? domain = null,
            string? location = null, string? owner = null, DateTimeOffset? lastSeenAt = null)
        {
            return new Host
            {
                Id = Threat.ValidateId(id),
                Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("name") : name.Trim(),
                IpAddress = ipAddress ?? throw new ArgumentException("ipAddress"),
                MacAddress = macAddress ?? throw new ArgumentException("macAddress"),
                Type = type,
                Status = status,
                OperatingSystem = operatingSystem,
                OsVersion = osVersion,
                Hostname = hostname,
                Domain = domain,
                Location = location,
                Tags = tags ?? throw new ArgumentException("tags"),
                Owner = owner,
                LastSeenAt = lastSeenAt,
                CreatedAt = createdAt
            };
        }
    }

    /// <summary>A network.</summary>
    public sealed class Network
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string Cidr { get; init; }
        public required string Description { get; init; }
        public required string Type { get; init; }
        public required IReadOnlyList<string> VlanIds { get; init; }
        public required IReadOnlyList<string> SubnetIds { get; init; }
        public required IReadOnlyList<string> Tags { get; init; }

        public static Network Create(
            string id, string name, string cidr, string description,
            string type, IReadOnlyList<string> tags,
            IReadOnlyList<string>? vlanIds = null, IReadOnlyList<string>? subnetIds = null)
        {
            return new Network
            {
                Id = Threat.ValidateId(id),
                Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("name") : name.Trim(),
                Cidr = cidr ?? throw new ArgumentException("cidr"),
                Description = description ?? throw new ArgumentException("description"),
                Type = type ?? throw new ArgumentException("type"),
                VlanIds = vlanIds ?? new List<string>(),
                SubnetIds = subnetIds ?? new List<string>(),
                Tags = tags ?? throw new ArgumentException("tags")
            };
        }
    }

    /// <summary>A subnet.</summary>
    public sealed class Subnet
    {
        public required string Id { get; init; }
        public required string NetworkId { get; init; }
        public required string Cidr { get; init; }
        public required string Name { get; init; }
        public required string Description { get; init; }
        public required IReadOnlyList<string> HostIds { get; init; }
        public required IReadOnlyList<string> Tags { get; init; }

        public static Subnet Create(
            string id, string networkId, string cidr, string name,
            string description, IReadOnlyList<string> tags,
            IReadOnlyList<string>? hostIds = null)
        {
            return new Subnet
            {
                Id = Threat.ValidateId(id),
                NetworkId = Threat.ValidateId(networkId),
                Cidr = cidr ?? throw new ArgumentException("cidr"),
                Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("name") : name.Trim(),
                Description = description ?? throw new ArgumentException("description"),
                HostIds = hostIds ?? new List<string>(),
                Tags = tags ?? throw new ArgumentException("tags")
            };
        }
    }

    /// <summary>A VLAN.</summary>
    public sealed class Vlan
    {
        public required string Id { get; init; }
        public required string NetworkId { get; init; }
        public required int VlanId { get; init; }
        public required string Name { get; init; }
        public required string Description { get; init; }
        public required IReadOnlyList<string> Tags { get; init; }

        public static Vlan Create(
            string id, string networkId, int vlanId, string name,
            string description, IReadOnlyList<string> tags)
        {
            return new Vlan
            {
                Id = Threat.ValidateId(id),
                NetworkId = Threat.ValidateId(networkId),
                VlanId = vlanId >= 0 && vlanId <= 4095 ? vlanId : throw new ArgumentException("vlanId"),
                Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("name") : name.Trim(),
                Description = description ?? throw new ArgumentException("description"),
                Tags = tags ?? throw new ArgumentException("tags")
            };
        }
    }

    /// <summary>A service.</summary>
    public sealed class Service
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string Description { get; init; }
        public required string Type { get; init; }
        public required IReadOnlyList<string> HostIds { get; init; }
        public required IReadOnlyList<int> Ports { get; init; }
        public required string Status { get; init; }
        public required string? Version { get; init; }
        public required string? Owner { get; init; }
        public required IReadOnlyList<string> Tags { get; init; }

        public static Service Create(
            string id, string name, string description, string type,
            IReadOnlyList<string> hostIds, IReadOnlyList<int> ports, string status,
            IReadOnlyList<string> tags, string? version = null, string? owner = null)
        {
            return new Service
            {
                Id = Threat.ValidateId(id),
                Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("name") : name.Trim(),
                Description = description ?? throw new ArgumentException("description"),
                Type = type ?? throw new ArgumentException("type"),
                HostIds = hostIds ?? throw new ArgumentException("hostIds"),
                Ports = ports ?? throw new ArgumentException("ports"),
                Status = status ?? throw new ArgumentException("status"),
                Version = version,
                Owner = owner,
                Tags = tags ?? throw new ArgumentException("tags")
            };
        }
    }

    /// <summary>An application.</summary>
    public sealed class Application
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string Description { get; init; }
        public required string Version { get; init; }
        public required string Vendor { get; init; }
        public required string Type { get; init; }
        public required IReadOnlyList<string> ServiceIds { get; init; }
        public required IReadOnlyList<string> HostIds { get; init; }
        public required string? Owner { get; init; }
        public required IReadOnlyList<string> Tags { get; init; }

        public static Application Create(
            string id, string name, string description, string version,
            string vendor, string type, IReadOnlyList<string> tags,
            IReadOnlyList<string>? serviceIds = null, IReadOnlyList<string>? hostIds = null,
            string? owner = null)
        {
            return new Application
            {
                Id = Threat.ValidateId(id),
                Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("name") : name.Trim(),
                Description = description ?? throw new ArgumentException("description"),
                Version = version ?? throw new ArgumentException("version"),
                Vendor = vendor ?? throw new ArgumentException("vendor"),
                Type = type ?? throw new ArgumentException("type"),
                ServiceIds = serviceIds ?? new List<string>(),
                HostIds = hostIds ?? new List<string>(),
                Owner = owner,
                Tags = tags ?? throw new ArgumentException("tags")
            };
        }
    }
}
