# SOCpit Data Model

## Overview

This document describes the complete data model for SOCpit - Security Operations Center. The data model is designed to support comprehensive security operations including threat detection, vulnerability management, incident response, evidence collection, and security monitoring.

## Entity Relationship Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                                SOCpit Data Model                                │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌─────────────────┐       ┌─────────────────┐       ┌─────────────────┐   │
│  │     THREAT      │       │  VULNERABILITY   │       │   INCIDENT      │   │
│  ├─────────────────┤       ├─────────────────┤       ├─────────────────┤   │
│  │ Id (PK)         │       │ Id (PK)         │       │ Id (PK)         │   │
│  │ Name            │       │ CveId           │       │ Title           │   │
│  │ Description     │       │ Name            │       │ Description     │   │
│  │ Severity       │       │ Description     │       │ Severity        │   │
│  │ Status          │       │ Severity        │       │ Status          │   │
│  │ TargetId (FK)   │       │ Status          │       │ AssignedTo      │   │
│  │ DetectedAt      │       │ TargetId (FK)   │       │ CreatedAt       │   │
│  │ Source          │       │ PublishedAt     │       │ UpdatedAt       │   │
│  │ Tags            │       │ LastUpdatedAt   │       │ ResolvedAt      │   │
│  │ MitigationSteps │       │ FixAvailable    │       │ ImpactAnalysis  │   │
│  │ RelatedEvidenceIds│      │ CvssScore       │       │ ResponseActions  │   │
│  └────────┬────────┘       │ References      │       │ LessonsLearned  │   │
│           │                 └────────┬────────┘       └────────┬────────┘   │
│           │                          │                         │                │
│           │                          │                         │                │
│           ▼                          ▼                         ▼                │
│  ┌─────────────────┐       ┌─────────────────┐       ┌─────────────────┐   │
│  │     HOST        │       │    NETWORK       │       │    EVIDENCE     │   │
│  ├─────────────────┤       ├─────────────────┤       ├─────────────────┤   │
│  │ Id (PK)         │       │ Id (PK)         │       │ Id (PK)         │   │
│  │ Name            │       │ Name            │       │ Name            │   │
│  │ IpAddress       │       │ Cidr            │       │ Type            │   │
│  │ MacAddress      │       │ Description     │       │ Description     │   │
│  │ Type            │       │ Type            │       │ Status          │   │
│  │ Status          │       │ VlanIds         │       │ Classification   │   │
│  │ OperatingSystem │       │ SubnetIds       │       │ Source          │   │
│  │ OsVersion       │       │ Tags            │       │ CollectedAt     │   │
│  │ Hostname        │       └─────────────────┘       │ SizeBytes       │   │
│  │ Domain          │                 │                 │ Hash            │   │
│  │ Location        │                 │                 │ FilePath        │   │
│  │ Tags            │                 │                 │ Tags            │   │
│  │ Owner           │                 │                 │ RelatedIncidentIds│  │
│  │ LastSeenAt      │                 │                 │ RelatedThreatIds │   │
│  │ CreatedAt       │                 │                 │ RelatedVulnerabilityIds││
│  └─────────────────┘                 │                 └────────┬────────┘   │
│                                      │                           │            │
│                                      │                           │            │
│  ┌─────────────────┐       ┌─────────────────┐       ▼            │
│  │     USER        │       │    SERVICE       │  ┌─────────────────┐   │
│  ├─────────────────┤       ├─────────────────┤  │   AUDIT LOG     │   │
│  │ Id (PK)         │       │ Id (PK)         │  ├─────────────────┤   │
│  │ Username        │       │ Name            │  │ Id (PK)         │   │
│  │ Email           │       │ Description     │  │ Action          │   │
│  │ FullName        │       │ Type            │  │ EntityType      │   │
│  │ Role            │       │ HostIds         │  │ EntityId        │   │
│  │ Status          │       │ Ports           │  │ UserId          │   │
│  │ AuthenticationType│      │ Status          │  │ Timestamp       │   │
│  │ CreatedAt       │       │ Version         │  │ Details         │   │
│  │ LastLoginAt     │       │ Vendor          │  │ OldValue        │   │
│  │ LockedUntil     │       │ Tags            │  │ NewValue        │   │
│  │ FailedLoginAttempts│   │ Owner           │  │ IpAddress       │   │
│  │ Department      │       └─────────────────┘  │ UserAgent       │   │
│  │ Phone           │                              │ Success         │   │
│  │ Tags            │                              │ ErrorMessage    │   │
│  │ PermissionIds    │                              └─────────────────┘   │
│  └─────────────────┘                                                │
│                                                                              │
│  ┌─────────────────┐       ┌─────────────────┐                              │
│  │   PERMISSION    │       │     ROLE        │                              │
│  ├─────────────────┤       ├─────────────────┤                              │
│  │ Id (PK)         │       │ Id (PK)         │                              │
│  │ Name            │       │ Name            │                              │
│  │ Description     │       │ Description     │                              │
│  │ Category        │       │ IsSystemRole    │                              │
│  │ ResourceType    │       │ PermissionIds    │                              │
│  │ Actions         │       └─────────────────┘                              │
│  │ IsSystemPermission│                                                        │
│  └─────────────────┘                                                        │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

## Entity Descriptions

### Security Entities

#### 1. Threat
Represents a security threat detected in the environment.

**Properties**:
- `Id`: Unique identifier (string, max 80 chars, alphanumeric with ._-)
- `Name`: Threat name (string, required, max 200 chars)
- `Description`: Detailed description (string, required)
- `Severity`: Threat severity level (enum: Low, Medium, High, Critical)
- `Status`: Current status (enum: New, Investigating, Contained, Resolved, FalsePositive)
- `TargetId`: ID of the target (host, network, etc.) (string, required, max 80 chars)
- `DetectedAt`: When the threat was detected (DateTimeOffset, required)
- `Source`: Source of the threat detection (string, required, max 100 chars)
- `Tags`: List of tags for categorization (IReadOnlyList<string>, required)
- `MitigationSteps`: Steps to mitigate the threat (string, nullable)
- `RelatedEvidenceIds`: IDs of related evidence (string, nullable)

**Relationships**:
- Belongs to a Target (Host, Network, etc.) via TargetId
- Can be linked to Evidence via RelatedEvidenceIds
- Can be linked to Incidents (indirectly via threat IDs)

**Validation**:
- Id must not be empty and must match pattern `[a-zA-Z0-9][a-zA-Z0-9._-]*`
- Name must not be empty
- Description must not be empty
- TargetId must not be empty
- Source must not be empty
- Tags must not be null

**Example**:
```json
{
  "Id": "threat-001",
  "Name": "Suspicious Login Attempt",
  "Description": "Multiple failed login attempts from IP 192.168.1.100",
  "Severity": "High",
  "Status": "Investigating",
  "TargetId": "host-001",
  "DetectedAt": "2026-10-09T10:00:00Z",
  "Source": "SIEM",
  "Tags": ["authentication", "brute-force"],
  "MitigationSteps": "Block IP address and reset affected user passwords",
  "RelatedEvidenceIds": "evidence-001,evidence-002"
}
```

#### 2. Vulnerability
Represents a vulnerability in the system.

**Properties**:
- `Id`: Unique identifier (string, required, max 80 chars)
- `CveId`: CVE identifier (string, required, max 50 chars)
- `Name`: Vulnerability name (string, required, max 200 chars)
- `Description`: Detailed description (string, required)
- `Severity`: Vulnerability severity (enum: Low, Medium, High, Critical)
- `Status`: Current status (enum: Open, InProgress, Fixed, NotApplicable, AcceptedRisk)
- `TargetId`: ID of the target (host, application, etc.) (string, required, max 80 chars)
- `PublishedAt`: When the vulnerability was published (DateTimeOffset, required)
- `LastUpdatedAt`: When the vulnerability was last updated (DateTimeOffset, nullable)
- `FixAvailable`: Whether a fix is available (string, nullable)
- `CvssScore`: CVSS score (string, nullable)
- `References`: List of reference URLs (IReadOnlyList<string>, required)

**Relationships**:
- Belongs to a Target (Host, Application, etc.) via TargetId
- Can be linked to Incidents (indirectly via vulnerability IDs)

**Example**:
```json
{
  "Id": "vuln-001",
  "CveId": "CVE-2024-1234",
  "Name": "Remote Code Execution in Web Server",
  "Description": "Remote code execution vulnerability in web server version 2.0",
  "Severity": "Critical",
  "Status": "Open",
  "TargetId": "host-001",
  "PublishedAt": "2024-01-15T00:00:00Z",
  "LastUpdatedAt": "2024-01-20T00:00:00Z",
  "FixAvailable": "Yes - Upgrade to version 2.1",
  "CvssScore": "9.8",
  "References": ["https://nvd.nist.gov/vuln/detail/CVE-2024-1234"]
}
```

#### 3. SecurityIncident
Represents a security incident.

**Properties**:
- `Id`: Unique identifier (string, required, max 80 chars)
- `Title`: Incident title (string, required, max 200 chars)
- `Description`: Detailed description (string, required)
- `Severity`: Incident severity (enum: Low, Medium, High, Critical)
- `Status`: Current status (enum: New, Triage, Investigating, Contained, Resolved, Closed)
- `CreatedAt`: When the incident was created (DateTimeOffset, required)
- `UpdatedAt`: When the incident was last updated (DateTimeOffset, nullable)
- `ResolvedAt`: When the incident was resolved (DateTimeOffset, nullable)
- `AssignedTo`: User ID of the assigned user (string, nullable, max 80 chars)
- `RelatedThreatIds`: List of related threat IDs (IReadOnlyList<string>, required)
- `RelatedVulnerabilityIds`: List of related vulnerability IDs (IReadOnlyList<string>, required)
- `ImpactAnalysis`: Analysis of the incident impact (string, nullable)
- `ResponseActions`: Actions taken in response (string, nullable)
- `LessonsLearned`: Lessons learned from the incident (string, nullable)

**Relationships**:
- Can be linked to Threats via RelatedThreatIds
- Can be linked to Vulnerabilities via RelatedVulnerabilityIds
- Can be assigned to a User via AssignedTo
- Can be linked to Evidence (indirectly)

**Example**:
```json
{
  "Id": "incident-001",
  "Title": "Data Breach Investigation",
  "Description": "Unauthorized access to customer database detected",
  "Severity": "Critical",
  "Status": "Investigating",
  "CreatedAt": "2026-10-09T10:00:00Z",
  "UpdatedAt": "2026-10-09T12:00:00Z",
  "ResolvedAt": null,
  "AssignedTo": "user-001",
  "RelatedThreatIds": ["threat-001", "threat-002"],
  "RelatedVulnerabilityIds": ["vuln-001"],
  "ImpactAnalysis": "Potential exposure of 10,000 customer records",
  "ResponseActions": "Isolated affected systems, began forensic investigation",
  "LessonsLearned": "Need to implement database encryption and better monitoring"
}
```

#### 4. Evidence
Represents a piece of evidence.

**Properties**:
- `Id`: Unique identifier (string, required, max 80 chars)
- `Name`: Evidence name (string, required, max 200 chars)
- `Type`: Evidence type (enum: Log, Screenshot, NetworkCapture, File, MemoryDump, Configuration, AuditLog, Alert, Report, Other)
- `Description`: Detailed description (string, required)
- `Status`: Current status (enum: New, Reviewed, Analyzed, Archived, Deleted)
- `Classification`: Classification level (enum: Public, Internal, Confidential, Restricted)
- `Source`: Source of the evidence (string, required, max 100 chars)
- `CollectedAt`: When the evidence was collected (DateTimeOffset, required)
- `SizeBytes`: Size of the evidence in bytes (long, required, >= 0)
- `Hash`: SHA256 hash of the evidence (string, required, max 64 chars)
- `FilePath`: Path to the evidence file (string, required, max 500 chars)
- `Tags`: List of tags for categorization (IReadOnlyList<string>, required)
- `RelatedIncidentIds`: IDs of related incidents (string, nullable)
- `RelatedThreatIds`: IDs of related threats (string, nullable)
- `RelatedVulnerabilityIds`: IDs of related vulnerabilities (string, nullable)
- `AnalysisNotes`: Analysis notes (string, nullable)
- `CollectedBy`: User ID of the collector (string, nullable, max 80 chars)

**Relationships**:
- Can be linked to Incidents via RelatedIncidentIds
- Can be linked to Threats via RelatedThreatIds
- Can be linked to Vulnerabilities via RelatedVulnerabilityIds
- Belongs to a User via CollectedBy

**Validation**:
- SizeBytes must be >= 0
- Hash must be a valid SHA256 hash (64 hex chars)
- FilePath must exist (when creating)

**Example**:
```json
{
  "Id": "evidence-001",
  "Name": "Suspicious Login Logs",
  "Type": "Log",
  "Description": "Logs showing multiple failed login attempts",
  "Status": "Reviewed",
  "Classification": "Internal",
  "Source": "SIEM",
  "CollectedAt": "2026-10-09T10:00:00Z",
  "SizeBytes": 1048576,
  "Hash": "2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824",
  "FilePath": "C:\\SOCpit2026\\evidence\\suspicious_logs.txt",
  "Tags": ["authentication", "brute-force"],
  "RelatedIncidentIds": "incident-001",
  "RelatedThreatIds": "threat-001",
  "RelatedVulnerabilityIds": null,
  "AnalysisNotes": "IP address 192.168.1.100 shows brute force pattern",
  "CollectedBy": "user-001"
}
```

#### 5. SecurityControl
Represents a security control.

**Properties**:
- `Id`: Unique identifier (string, required, max 80 chars)
- `Name`: Control name (string, required, max 200 chars)
- `Description`: Detailed description (string, required)
- `Type`: Control type (enum: Preventive, Detective, Corrective, Deterrent, Recovery, Compensating)
- `Category`: Control category (string, required, max 100 chars)
- `Required`: Whether the control is required (bool, required)
- `Scope`: Scope of the control (string, required, max 200 chars)
- `Implementation`: Implementation details (string, required)
- `VerificationMethod`: Method to verify the control (string, nullable)
- `FrequencyDays`: Frequency of verification in days (int, required, >= 1)
- `LastVerifiedAt`: When the control was last verified (DateTimeOffset, required)
- `Owner`: User ID of the control owner (string, nullable, max 80 chars)

**Example**:
```json
{
  "Id": "control-001",
  "Name": "Firewall Protection",
  "Description": "Firewall to protect network perimeter",
  "Type": "Preventive",
  "Category": "Network Security",
  "Required": true,
  "Scope": "All external-facing systems",
  "Implementation": "Cisco ASA firewall with specific rules",
  "VerificationMethod": "Automated test suite",
  "FrequencyDays": 30,
  "LastVerifiedAt": "2026-10-01T00:00:00Z",
  "Owner": "user-001"
}
```

#### 6. ComplianceCheck
Represents a compliance check.

**Properties**:
- `Id`: Unique identifier (string, required, max 80 chars)
- `Standard`: Compliance standard (string, required, max 100 chars)
- `Requirement`: Specific requirement (string, required, max 200 chars)
- `Status`: Compliance status (enum: Compliant, NonCompliant, Partial, NotApplicable, NotAssessed)
- `TargetId`: ID of the target (string, required, max 80 chars)
- `CheckedAt`: When the check was performed (DateTimeOffset, required)
- `NextCheckAt`: When the next check is due (DateTimeOffset, nullable)
- `EvidenceId`: ID of supporting evidence (string, nullable, max 80 chars)
- `Notes`: Additional notes (string, nullable)
- `NonComplianceDetails`: Details of non-compliance (string, nullable)

**Example**:
```json
{
  "Id": "check-001",
  "Standard": "ISO 27001",
  "Requirement": "Access control policy and procedures",
  "Status": "Compliant",
  "TargetId": "host-001",
  "CheckedAt": "2026-10-01T00:00:00Z",
  "NextCheckAt": "2027-01-01T00:00:00Z",
  "EvidenceId": "evidence-001",
  "Notes": "Policy reviewed and updated",
  "NonComplianceDetails": null
}
```

### Infrastructure Entities

#### 7. Host
Represents a host asset (server, workstation, network device, etc.).

**Properties**:
- `Id`: Unique identifier (string, required, max 80 chars)
- `Name`: Host name (string, required, max 200 chars)
- `IpAddress`: IP address (string, required, max 45 chars)
- `MacAddress`: MAC address (string, required, max 17 chars)
- `Type`: Asset type (enum: Server, Workstation, NetworkDevice, Application, Database, CloudResource, IoTDevice, MobileDevice, Other)
- `Status`: Asset status (enum: Active, Inactive, Retired, Maintenance, Unknown)
- `OperatingSystem`: Operating system type (enum: Windows, Linux, macOS, Unix, iOS, Android, Other)
- `OsVersion`: Operating system version (string, nullable, max 50 chars)
- `Hostname`: Hostname (string, nullable, max 255 chars)
- `Domain`: Domain name (string, nullable, max 255 chars)
- `Location`: Physical location (string, nullable, max 200 chars)
- `Tags`: List of tags for categorization (IReadOnlyList<string>, required)
- `Owner`: User ID of the owner (string, nullable, max 80 chars)
- `LastSeenAt`: When the host was last seen (DateTimeOffset, nullable)
- `CreatedAt`: When the host was created (DateTimeOffset, required)

**Relationships**:
- Can have Vulnerabilities (via TargetId in Vulnerability)
- Can have Threats (via TargetId in Threat)
- Can belong to Networks (via Subnet)
- Can have Services running on it
- Can have Applications installed

**Example**:
```json
{
  "Id": "host-001",
  "Name": "Web Server 01",
  "IpAddress": "192.168.1.10",
  "MacAddress": "00:11:22:33:44:55",
  "Type": "Server",
  "Status": "Active",
  "OperatingSystem": "Windows",
  "OsVersion": "Windows Server 2022",
  "Hostname": "web01.example.com",
  "Domain": "example.com",
  "Location": "Data Center 1",
  "Tags": ["web", "production", "critical"],
  "Owner": "user-001",
  "LastSeenAt": "2026-10-09T10:00:00Z",
  "CreatedAt": "2024-01-01T00:00:00Z"
}
```

#### 8. Network
Represents a network.

**Properties**:
- `Id`: Unique identifier (string, required, max 80 chars)
- `Name`: Network name (string, required, max 200 chars)
- `Cidr`: Network CIDR (string, required, max 18 chars)
- `Description`: Detailed description (string, required)
- `Type`: Network type (string, required, max 50 chars)
- `VlanIds`: List of VLAN IDs (IReadOnlyList<string>, required)
- `SubnetIds`: List of subnet IDs (IReadOnlyList<string>, required)
- `Tags`: List of tags for categorization (IReadOnlyList<string>, required)

**Relationships**:
- Contains Subnets (via SubnetIds)
- Contains VLANs (via VlanIds)
- Contains Hosts (via Subnet)

**Example**:
```json
{
  "Id": "network-001",
  "Name": "Corporate Network",
  "Cidr": "192.168.0.0/16",
  "Description": "Main corporate network",
  "Type": "Internal",
  "VlanIds": ["vlan-001", "vlan-002"],
  "SubnetIds": ["subnet-001", "subnet-002", "subnet-003"],
  "Tags": ["corporate", "production"]
}
```

#### 9. Subnet
Represents a subnet within a network.

**Properties**:
- `Id`: Unique identifier (string, required, max 80 chars)
- `NetworkId`: ID of the parent network (string, required, max 80 chars)
- `Cidr`: Subnet CIDR (string, required, max 18 chars)
- `Name`: Subnet name (string, required, max 200 chars)
- `Description`: Detailed description (string, required)
- `HostIds`: List of host IDs in this subnet (IReadOnlyList<string>, required)
- `Tags`: List of tags for categorization (IReadOnlyList<string>, required)

**Relationships**:
- Belongs to a Network (via NetworkId)
- Contains Hosts (via HostIds)

**Example**:
```json
{
  "Id": "subnet-001",
  "NetworkId": "network-001",
  "Cidr": "192.168.1.0/24",
  "Name": "Web Servers Subnet",
  "Description": "Subnet for web servers",
  "HostIds": ["host-001", "host-002"],
  "Tags": ["web", "servers"]
}
```

#### 10. Vlan
Represents a VLAN within a network.

**Properties**:
- `Id`: Unique identifier (string, required, max 80 chars)
- `NetworkId`: ID of the parent network (string, required, max 80 chars)
- `VlanId`: VLAN ID (int, required, 0-4095)
- `Name`: VLAN name (string, required, max 200 chars)
- `Description`: Detailed description (string, required)
- `Tags`: List of tags for categorization (IReadOnlyList<string>, required)

**Relationships**:
- Belongs to a Network (via NetworkId)

**Example**:
```json
{
  "Id": "vlan-001",
  "NetworkId": "network-001",
  "VlanId": 10,
  "Name": "Web VLAN",
  "Description": "VLAN for web servers",
  "Tags": ["web"]
}
```

#### 11. Service
Represents a service running on one or more hosts.

**Properties**:
- `Id`: Unique identifier (string, required, max 80 chars)
- `Name`: Service name (string, required, max 200 chars)
- `Description`: Detailed description (string, required)
- `Type`: Service type (string, required, max 100 chars)
- `HostIds`: List of host IDs running this service (IReadOnlyList<string>, required)
- `Ports`: List of ports used by this service (IReadOnlyList<int>, required)
- `Status`: Service status (string, required, max 50 chars)
- `Version`: Service version (string, nullable, max 50 chars)
- `Owner`: User ID of the service owner (string, nullable, max 80 chars)
- `Tags`: List of tags for categorization (IReadOnlyList<string>, required)

**Relationships**:
- Runs on Hosts (via HostIds)
- Can belong to Applications

**Example**:
```json
{
  "Id": "service-001",
  "Name": "Web Service",
  "Description": "Main web application service",
  "Type": "HTTP",
  "HostIds": ["host-001", "host-002"],
  "Ports": [80, 443],
  "Status": "Running",
  "Version": "2.0.0",
  "Owner": "user-001",
  "Tags": ["web", "production"]
}
```

#### 12. Application
Represents an application.

**Properties**:
- `Id`: Unique identifier (string, required, max 80 chars)
- `Name`: Application name (string, required, max 200 chars)
- `Description`: Detailed description (string, required)
- `Version`: Application version (string, required, max 50 chars)
- `Vendor`: Application vendor (string, required, max 100 chars)
- `Type`: Application type (string, required, max 100 chars)
- `ServiceIds`: List of service IDs belonging to this application (IReadOnlyList<string>, required)
- `HostIds`: List of host IDs running this application (IReadOnlyList<string>, required)
- `Owner`: User ID of the application owner (string, nullable, max 80 chars)
- `Tags`: List of tags for categorization (IReadOnlyList<string>, required)

**Relationships**:
- Contains Services (via ServiceIds)
- Runs on Hosts (via HostIds)
- Can have Vulnerabilities (via TargetId in Vulnerability)

**Example**:
```json
{
  "Id": "app-001",
  "Name": "Customer Portal",
  "Description": "Customer-facing web portal",
  "Version": "3.1.0",
  "Vendor": "Example Corp",
  "Type": "Web Application",
  "ServiceIds": ["service-001", "service-002"],
  "HostIds": ["host-001", "host-002"],
  "Owner": "user-001",
  "Tags": ["customer", "portal", "web"]
}
```

#### 13. NetworkConnection
Represents a network connection.

**Properties**:
- `Id`: Unique identifier (string, required, max 80 chars)
- `SourceIp`: Source IP address (string, required, max 45 chars)
- `SourcePort`: Source port (int, required, 0-65535)
- `DestinationIp`: Destination IP address (string, required, max 45 chars)
- `DestinationPort`: Destination port (int, required, 0-65535)
- `Protocol`: Network protocol (enum: TCP, UDP, ICMP, HTTP, HTTPS, FTP, SSH, DNS, DHCP, SMTP, Other)
- `EstablishedAt`: When the connection was established (DateTimeOffset, required)
- `TerminatedAt`: When the connection was terminated (DateTimeOffset, nullable)
- `BytesSent`: Number of bytes sent (long, required, >= 0)
- `BytesReceived`: Number of bytes received (long, required, >= 0)
- `ProcessName`: Name of the process (string, nullable, max 255 chars)
- `UserId`: User ID associated with the connection (string, nullable, max 80 chars)

**Example**:
```json
{
  "Id": "conn-001",
  "SourceIp": "192.168.1.10",
  "SourcePort": 12345,
  "DestinationIp": "192.168.1.20",
  "DestinationPort": 443,
  "Protocol": "TCP",
  "EstablishedAt": "2026-10-09T10:00:00Z",
  "TerminatedAt": "2026-10-09T10:05:00Z",
  "BytesSent": 1024,
  "BytesReceived": 2048,
  "ProcessName": "chrome.exe",
  "UserId": "user-001"
}
```

#### 14. LogEntry
Represents a log entry.

**Properties**:
- `Id`: Unique identifier (string, required, max 80 chars)
- `Source`: Log source (string, required, max 100 chars)
- `Message`: Log message (string, required)
- `Level`: Log level (string, required, max 20 chars)
- `Timestamp`: When the log entry was created (DateTimeOffset, required)
- `Host`: Hostname (string, nullable, max 255 chars)
- `Application`: Application name (string, nullable, max 100 chars)
- `ProcessId`: Process ID (string, nullable, max 20 chars)
- `ThreadId`: Thread ID (string, nullable, max 20 chars)
- `Exception`: Exception details (string, nullable)
- `Properties`: Additional properties (IReadOnlyDictionary<string, string>, nullable)

**Example**:
```json
{
  "Id": "log-001",
  "Source": "Application",
  "Message": "User login successful",
  "Level": "Information",
  "Timestamp": "2026-10-09T10:00:00Z",
  "Host": "web01.example.com",
  "Application": "Customer Portal",
  "ProcessId": "1234",
  "ThreadId": "5678",
  "Exception": null,
  "Properties": {
    "UserId": "user-001",
    "IpAddress": "192.168.1.100"
  }
}
```

#### 15. Report
Represents a report.

**Properties**:
- `Id`: Unique identifier (string, required, max 80 chars)
- `Title`: Report title (string, required, max 200 chars)
- `Type`: Report type (string, required, max 100 chars)
- `Content`: Report content (string, required)
- `GeneratedAt`: When the report was generated (DateTimeOffset, required)
- `GeneratedBy`: User ID of the generator (string, required, max 80 chars)
- `TemplateId`: ID of the template used (string, nullable, max 80 chars)
- `Parameters`: List of parameters used (IReadOnlyList<string>, required)
- `FilePath`: Path to the report file (string, required, max 500 chars)

**Example**:
```json
{
  "Id": "report-001",
  "Title": "Threat Summary - October 2026",
  "Type": "Threat Summary",
  "Content": "... report content ...",
  "GeneratedAt": "2026-10-09T10:00:00Z",
  "GeneratedBy": "user-001",
  "TemplateId": "template-001",
  "Parameters": ["startDate=2026-10-01", "endDate=2026-10-09"],
  "FilePath": "C:\\SOCpit2026\\reports\\threat_summary_2026_10_09.pdf"
}
```

### User & Identity Entities

#### 16. User
Represents a user account.

**Properties**:
- `Id`: Unique identifier (string, required, max 80 chars)
- `Username`: Username (string, required, max 80 chars)
- `Email`: Email address (string, required, max 255 chars)
- `FullName`: Full name (string, required, max 200 chars)
- `Role`: User role (enum: Viewer, Analyst, Investigator, Administrator, SuperAdministrator)
- `Status`: User status (enum: Active, Inactive, Locked, Disabled, Pending)
- `AuthenticationType`: Authentication type (enum: Local, LDAP, ActiveDirectory, OAuth, SAML, Certificate, MultiFactor)
- `CreatedAt`: When the user was created (DateTimeOffset, required)
- `LastLoginAt`: When the user last logged in (DateTimeOffset, nullable)
- `LockedUntil`: When the lockout expires (DateTimeOffset, nullable)
- `FailedLoginAttempts`: Number of failed login attempts (int, required, >= 0)
- `Department`: Department (string, nullable, max 100 chars)
- `Phone`: Phone number (string, nullable, max 20 chars)
- `Tags`: List of tags for categorization (IReadOnlyList<string>, required)
- `PermissionIds`: List of permission IDs (IReadOnlyList<string>, required)

**Relationships**:
- Has many Permissions (via PermissionIds)
- Can belong to Groups
- Can have Sessions
- Can be assigned to Incidents
- Can collect Evidence

**Example**:
```json
{
  "Id": "user-001",
  "Username": "john.doe",
  "Email": "john.doe@example.com",
  "FullName": "John Doe",
  "Role": "Analyst",
  "Status": "Active",
  "AuthenticationType": "Local",
  "CreatedAt": "2024-01-01T00:00:00Z",
  "LastLoginAt": "2026-10-09T10:00:00Z",
  "LockedUntil": null,
  "FailedLoginAttempts": 0,
  "Department": "Security",
  "Phone": "+1-555-123-4567",
  "Tags": ["analyst", "team-a"],
  "PermissionIds": ["perm-001", "perm-002"]
}
```

#### 17. Permission
Represents a permission.

**Properties**:
- `Id`: Unique identifier (string, required, max 80 chars)
- `Name`: Permission name (string, required, max 100 chars)
- `Description`: Detailed description (string, required)
- `Category`: Permission category (string, required, max 50 chars)
- `ResourceType`: Type of resource the permission applies to (string, required, max 50 chars)
- `Actions`: List of allowed actions (IReadOnlyList<string>, required)
- `IsSystemPermission`: Whether this is a system permission (bool, required)

**Relationships**:
- Can be assigned to Users
- Can be assigned to Roles

**Example**:
```json
{
  "Id": "perm-001",
  "Name": "Threat:Read",
  "Description": "Permission to view threats",
  "Category": "Threat Management",
  "ResourceType": "Threat",
  "Actions": ["Read", "List"],
  "IsSystemPermission": true
}
```

#### 18. Role
Represents a role.

**Properties**:
- `Id`: Unique identifier (string, required, max 80 chars)
- `Name`: Role name (string, required, max 100 chars)
- `Description`: Detailed description (string, required)
- `IsSystemRole`: Whether this is a system role (bool, required)
- `PermissionIds`: List of permission IDs (IReadOnlyList<string>, required)

**Relationships**:
- Has many Permissions (via PermissionIds)
- Can be assigned to Users (indirectly via User.Role)
- Can be assigned to Groups

**Example**:
```json
{
  "Id": "role-001",
  "Name": "Analyst",
  "Description": "Analyst role with read/write access to threats and vulnerabilities",
  "IsSystemRole": true,
  "PermissionIds": ["perm-001", "perm-002", "perm-003"]
}
```

#### 19. Group
Represents a group of users.

**Properties**:
- `Id`: Unique identifier (string, required, max 80 chars)
- `Name`: Group name (string, required, max 100 chars)
- `Description`: Detailed description (string, required)
- `UserIds`: List of user IDs in this group (IReadOnlyList<string>, required)
- `RoleIds`: List of role IDs assigned to this group (IReadOnlyList<string>, required)
- `PermissionIds`: List of permission IDs assigned to this group (IReadOnlyList<string>, required)

**Relationships**:
- Contains Users (via UserIds)
- Has many Roles (via RoleIds)
- Has many Permissions (via PermissionIds)

**Example**:
```json
{
  "Id": "group-001",
  "Name": "Security Team",
  "Description": "Security team with full access to security features",
  "UserIds": ["user-001", "user-002", "user-003"],
  "RoleIds": ["role-002"],
  "PermissionIds": ["perm-001", "perm-002"]
}
```

#### 20. Session
Represents a user session.

**Properties**:
- `Id`: Unique identifier (string, required, max 80 chars)
- `UserId`: ID of the associated user (string, required, max 80 chars)
- `Token`: Session token (string, required, max 500 chars)
- `CreatedAt`: When the session was created (DateTimeOffset, required)
- `ExpiresAt`: When the session expires (DateTimeOffset, required)
- `IpAddress`: IP address of the session (string, required, max 45 chars)
- `UserAgent`: User agent/browser (string, required, max 500 chars)
- `IsActive`: Whether the session is active (bool, required)
- `RefreshToken`: Refresh token (string, nullable, max 500 chars)

**Relationships**:
- Belongs to a User (via UserId)

**Example**:
```json
{
  "Id": "session-001",
  "UserId": "user-001",
  "Token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "CreatedAt": "2026-10-09T10:00:00Z",
  "ExpiresAt": "2026-10-09T10:30:00Z",
  "IpAddress": "192.168.1.100",
  "UserAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
  "IsActive": true,
  "RefreshToken": "dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4..."
}
```

#### 21. UserAuditEntry
Represents an audit entry for user actions.

**Properties**:
- `Id`: Unique identifier (string, required, max 80 chars)
- `UserId`: ID of the user (string, required, max 80 chars)
- `Action`: Action performed (string, required, max 100 chars)
- `Timestamp`: When the action was performed (DateTimeOffset, required)
- `Details`: Additional details (string, nullable)
- `IpAddress`: IP address (string, nullable, max 45 chars)
- `UserAgent`: User agent/browser (string, nullable, max 500 chars)
- `Success`: Whether the action succeeded (bool, required)

**Relationships**:
- Belongs to a User (via UserId)

**Example**:
```json
{
  "Id": "audit-001",
  "UserId": "user-001",
  "Action": "Login",
  "Timestamp": "2026-10-09T10:00:00Z",
  "Details": "User logged in successfully",
  "IpAddress": "192.168.1.100",
  "UserAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64)",
  "Success": true
}
```

## Enumerations

### ThreatSeverity
- `Low` - Low severity threat
- `Medium` - Medium severity threat
- `High` - High severity threat
- `Critical` - Critical severity threat

### ThreatStatus
- `New` - Newly detected threat
- `Investigating` - Threat is being investigated
- `Contained` - Threat has been contained
- `Resolved` - Threat has been resolved
- `FalsePositive` - Threat is a false positive

### VulnerabilitySeverity
- `Low` - Low severity vulnerability
- `Medium` - Medium severity vulnerability
- `High` - High severity vulnerability
- `Critical` - Critical severity vulnerability

### VulnerabilityStatus
- `Open` - Vulnerability is open
- `InProgress` - Vulnerability is being addressed
- `Fixed` - Vulnerability has been fixed
- `NotApplicable` - Vulnerability is not applicable
- `AcceptedRisk` - Vulnerability is an accepted risk

### IncidentSeverity
- `Low` - Low severity incident
- `Medium` - Medium severity incident
- `High` - High severity incident
- `Critical` - Critical severity incident

### IncidentStatus
- `New` - Newly created incident
- `Triage` - Incident is being triaged
- `Investigating` - Incident is being investigated
- `Contained` - Incident has been contained
- `Resolved` - Incident has been resolved
- `Closed` - Incident has been closed

### EvidenceType
- `Log` - Log file
- `Screenshot` - Screenshot
- `NetworkCapture` - Network traffic capture
- `File` - Generic file
- `MemoryDump` - Memory dump
- `Configuration` - Configuration file
- `AuditLog` - Audit log
- `Alert` - Alert
- `Report` - Report
- `Other` - Other type

### EvidenceStatus
- `New` - Newly collected evidence
- `Reviewed` - Evidence has been reviewed
- `Analyzed` - Evidence has been analyzed
- `Archived` - Evidence has been archived
- `Deleted` - Evidence has been deleted

### EvidenceClassification
- `Public` - Public information
- `Internal` - Internal organization information
- `Confidential` - Confidential information
- `Restricted` - Restricted information

### AssetType
- `Server` - Server
- `Workstation` - Workstation
- `NetworkDevice` - Network device
- `Application` - Application
- `Database` - Database
- `CloudResource` - Cloud resource
- `IoTDevice` - IoT device
- `MobileDevice` - Mobile device
- `Other` - Other asset type

### AssetStatus
- `Active` - Asset is active
- `Inactive` - Asset is inactive
- `Retired` - Asset is retired
- `Maintenance` - Asset is in maintenance
- `Unknown` - Asset status is unknown

### OperatingSystemType
- `Windows` - Microsoft Windows
- `Linux` - Linux
- `macOS` - Apple macOS
- `Unix` - Unix
- `iOS` - Apple iOS
- `Android` - Google Android
- `Other` - Other operating system

### NetworkProtocol
- `TCP` - Transmission Control Protocol
- `UDP` - User Datagram Protocol
- `ICMP` - Internet Control Message Protocol
- `HTTP` - Hypertext Transfer Protocol
- `HTTPS` - HTTP Secure
- `FTP` - File Transfer Protocol
- `SSH` - Secure Shell
- `DNS` - Domain Name System
- `DHCP` - Dynamic Host Configuration Protocol
- `SMTP` - Simple Mail Transfer Protocol
- `Other` - Other protocol

### UserRole
- `Viewer` - Read-only access
- `Analyst` - Read/write access to threats and vulnerabilities
- `Investigator` - Read/write access to all security entities
- `Administrator` - Full access except user management
- `SuperAdministrator` - Full access to all features

### UserStatus
- `Active` - User is active
- `Inactive` - User is inactive
- `Locked` - User is locked
- `Disabled` - User is disabled
- `Pending` - User registration is pending

### AuthenticationType
- `Local` - Local authentication
- `LDAP` - LDAP authentication
- `ActiveDirectory` - Active Directory authentication
- `OAuth` - OAuth authentication
- `SAML` - SAML authentication
- `Certificate` - Certificate-based authentication
- `MultiFactor` - Multi-factor authentication

### SecurityControlType
- `Preventive` - Preventive control
- `Detective` - Detective control
- `Corrective` - Corrective control
- `Deterrent` - Deterrent control
- `Recovery` - Recovery control
- `Compensating` - Compensating control

### ComplianceStatus
- `Compliant` - Compliant
- `NonCompliant` - Non-compliant
- `Partial` - Partially compliant
- `NotApplicable` - Not applicable
- `NotAssessed` - Not yet assessed

## Database Schema

### Tables

The database schema is implemented using Entity Framework Core with the following tables:

1. **Threats** - Stores threat information
2. **Vulnerabilities** - Stores vulnerability information
3. **SecurityIncidents** - Stores security incident information
4. **Evidence** - Stores evidence information
5. **Hosts** - Stores host information
6. **Networks** - Stores network information
7. **Subnets** - Stores subnet information
8. **Vlans** - Stores VLAN information
9. **Services** - Stores service information
10. **Applications** - Stores application information
11. **Users** - Stores user information
12. **Permissions** - Stores permission information
13. **Roles** - Stores role information
14. **Groups** - Stores group information
15. **Sessions** - Stores session information
16. **AuditLogs** - Stores audit log entries
17. **UserAuditEntries** - Stores user audit entries
18. **SecurityControls** - Stores security control information
19. **ComplianceChecks** - Stores compliance check information
20. **LogEntries** - Stores log entries
21. **Reports** - Stores report information
22. **NetworkConnections** - Stores network connection information

### Indexes

Primary keys:
- All tables have a primary key on the `Id` column

Foreign keys:
- Threats.TargetId → Hosts.Id, Networks.Id, etc.
- Vulnerabilities.TargetId → Hosts.Id, Applications.Id, etc.
- SecurityIncidents.AssignedTo → Users.Id
- Evidence.CollectedBy → Users.Id
- Hosts.Owner → Users.Id
- Networks.SubnetIds → Subnets.Id
- Networks.VlanIds → Vlans.Id
- Subnets.NetworkId → Networks.Id
- Subnets.HostIds → Hosts.Id
- Services.HostIds → Hosts.Id
- Applications.ServiceIds → Services.Id
- Applications.HostIds → Hosts.Id
- Users.PermissionIds → Permissions.Id
- Roles.PermissionIds → Permissions.Id
- Groups.UserIds → Users.Id
- Groups.RoleIds → Roles.Id
- Groups.PermissionIds → Permissions.Id
- Sessions.UserId → Users.Id
- AuditLogs.UserId → Users.Id
- UserAuditEntries.UserId → Users.Id

### Constraints

- All required fields are marked as NOT NULL
- String fields have maximum length constraints
- Enum fields are stored as integers with conversion
- DateTimeOffset fields are stored as UTC

## Data Flow

### Create Operations
1. User action in View
2. ViewModel receives action
3. ViewModel creates Command
4. Command is handled by CommandHandler
5. CommandHandler calls Service
6. Service validates and processes data
7. Service calls Repository
8. Repository saves to Database via DbContext
9. UnitOfWork commits transaction
10. Audit log is created
11. Result is returned to ViewModel
12. ViewModel updates View

### Read Operations
1. User action in View
2. ViewModel receives action
3. ViewModel creates Query
4. Query is handled by QueryHandler
5. QueryHandler calls Service
6. Service calls Repository
7. Repository queries Database via DbContext
8. Results are returned to Service
9. Service processes results
10. Results are returned to ViewModel
11. ViewModel updates View

### Update Operations
Similar to Create, but:
1. Repository retrieves existing entity
2. Service validates update
3. Repository updates entity
4. UnitOfWork commits transaction
5. Audit log records the change

### Delete Operations
Similar to Create, but:
1. Repository retrieves existing entity
2. Service validates deletion
3. Repository deletes entity
4. UnitOfWork commits transaction
5. Audit log records the deletion

## Validation Rules

### Common Validation
- All IDs must match pattern: `[a-zA-Z0-9][a-zA-Z0-9._-]*`
- All IDs must be 1-80 characters
- All required string fields must not be empty or whitespace
- All collection fields must not be null

### Entity-Specific Validation

**Threat**:
- Name: 1-200 characters
- Description: Required
- TargetId: Required, 1-80 characters
- Source: Required, 1-100 characters

**Vulnerability**:
- CveId: Required, 1-50 characters
- Name: 1-200 characters
- Description: Required
- TargetId: Required, 1-80 characters

**SecurityIncident**:
- Title: 1-200 characters
- Description: Required
- AssignedTo: 0-80 characters (nullable)

**Evidence**:
- Name: 1-200 characters
- Type: Required
- Description: Required
- Source: Required, 1-100 characters
- SizeBytes: >= 0
- Hash: Required, 64 hex characters
- FilePath: Required, 1-500 characters

**Host**:
- Name: 1-200 characters
- IpAddress: Required, valid IP address format, 1-45 characters
- MacAddress: Required, valid MAC address format, 1-17 characters
- Hostname: 0-255 characters (nullable)
- Domain: 0-255 characters (nullable)
- Location: 0-200 characters (nullable)
- Owner: 0-80 characters (nullable)

**Network**:
- Name: 1-200 characters
- Cidr: Required, valid CIDR format, 1-18 characters
- Description: Required
- Type: Required, 1-50 characters

**User**:
- Username: Required, 1-80 characters
- Email: Required, valid email format, 1-255 characters
- FullName: Required, 1-200 characters
- Department: 0-100 characters (nullable)
- Phone: 0-20 characters (nullable)

## Query Patterns

### Common Queries

**Get by ID**:
```csharp
var threat = await threatRepository.GetByIdAsync("threat-001");
```

**Get all**:
```csharp
var threats = await threatRepository.GetAllAsync();
```

**Get by filter**:
```csharp
var criticalThreats = await threatRepository.GetBySeverityAsync(ThreatSeverity.Critical);
```

**Find by predicate**:
```csharp
var recentThreats = await threatRepository.FindAsync(t => t.DetectedAt > DateTimeOffset.UtcNow.AddDays(-7));
```

**Count**:
```csharp
var threatCount = await threatRepository.CountAsync();
```

**Any**:
```csharp
var hasCritical = await threatRepository.AnyAsync(t => t.Severity == ThreatSeverity.Critical);
```

### Complex Queries

**Get threats with related evidence**:
```csharp
var threatsWithEvidence = await dbContext.Threats
    .Where(t => t.Severity == ThreatSeverity.Critical)
    .Include(t => t.RelatedEvidenceIds) // Note: This would need to be properly configured
    .ToListAsync();
```

**Get incidents with related threats and vulnerabilities**:
```csharp
var incidents = await dbContext.SecurityIncidents
    .Where(i => i.Status == IncidentStatus.New)
    .Include(i => i.RelatedThreatIds)
    .Include(i => i.RelatedVulnerabilityIds)
    .ToListAsync();
```

**Get hosts with vulnerabilities**:
```csharp
var vulnerableHosts = await dbContext.Hosts
    .Where(h => h.Vulnerabilities.Any(v => v.Severity == VulnerabilitySeverity.Critical))
    .ToListAsync();
```

## Migration Strategy

### Initial Migration
The initial migration creates all tables with the required schema. Entity Framework Core's `EnsureCreated()` is used for development, but proper migrations should be used for production.

### Migration Steps

1. **Create Migration**:
   ```bash
   dotnet ef migrations add InitialCreate --project SOCpit.Storage
   ```

2. **Update Database**:
   ```bash
   dotnet ef database update --project SOCpit.Storage
   ```

3. **Apply to Production**:
   ```bash
   dotnet ef database update --project SOCpit.Storage --connection "Data Source=production.db"
   ```

### Migration Files
Migrations are stored in the `SOCpit.Storage/Migrations` directory and include:
- `YYYYMMDDHHMMSS_InitialCreate.cs` - Initial migration
- `YYYYMMDDHHMMSS_InitialCreate.Designer.cs` - Migration metadata
- Additional migrations for schema changes

## Data Storage

### Default Storage
- **Location**: `%LocalAppData%\SOCpit2026\socpit.db`
- **Type**: SQLite database
- **Format**: Single file

### Custom Storage
The data storage path can be configured in the Settings view or via the `DatabasePath` constant in `App.xaml.cs`.

### Backup Strategy
- Automatic backups can be configured in Settings
- Backup files are stored in a separate directory
- Backup frequency is configurable
- Backup retention period is configurable

## Data Export/Import

### Export Formats
- **JSON**: Standard export format for all entities
- **CSV**: Tabular export for reports (future)
- **PDF**: Formatted reports (future)

### Import Formats
- **JSON**: Standard import format for all entities
- **MATRIX**: Direct import from MATRIX V3

### Export/Import Process
1. Select entities to export/import
2. Specify export/import path
3. Configure export/import options
4. Execute export/import
5. Verify results

## Conclusion

The SOCpit data model provides a comprehensive foundation for a Security Operations Center, supporting all aspects of security operations including threat detection, vulnerability management, incident response, evidence collection, and security monitoring. The model is designed to be:

- **Flexible**: Can be extended to support new requirements
- **Scalable**: Can handle large volumes of data
- **Secure**: Includes built-in security features
- **Auditability**: Comprehensive audit logging
- **Integratable**: Can integrate with external systems (MATRIX, SIEM, etc.)

This data model enables SOCpit to effectively manage security operations and provide valuable insights into the organization's security posture.
