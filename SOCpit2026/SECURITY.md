# SOCpit Security

## Security Policy

This document outlines the security policies and practices for the SOCpit Security Operations Center application.

### ADR-V2-SEC-001: Conservative Security Policy

**Status**: Accepted  
**Decision**: Implement a conservative security policy with defense-in-depth approach.  
**Context**: SOCpit handles sensitive security data including threats, vulnerabilities, incidents, and evidence. A security breach could have severe consequences for the organization's security posture.  

**Decision**: 
- All data is treated as sensitive by default
- Defense-in-depth approach with multiple security layers
- Principle of least privilege for all operations
- Zero-trust architecture where possible
- Security by design and security by default

**Consequences**:
- More complex implementation
- Potential performance overhead from security checks
- Better protection against data breaches
- Compliance with security standards

## Security Architecture

### 1. Authentication & Authorization

#### Authentication Types
SOCpit supports multiple authentication mechanisms:

| Type | Description | Security Level | Use Case |
|------|-------------|----------------|----------|
| Local | Username/password stored in database | Medium | Single-user, development |
| LDAP | Lightweight Directory Access Protocol | High | Enterprise environments |
| Active Directory | Microsoft Active Directory integration | High | Windows environments |
| OAuth 2.0 | OAuth 2.0 / OpenID Connect | High | Cloud environments |
| SAML | Security Assertion Markup Language | High | Enterprise SSO |
| Certificate | X.509 certificate-based authentication | Very High | High-security environments |
| Multi-Factor | MFA with TOTP or hardware tokens | Very High | Maximum security |

#### Role-Based Access Control (RBAC)

**Roles**:
```
SuperAdministrator (Full access to all features and data)
    ↓
Administrator (Full access except user management)
    ↓
Investigator (Read/write access to threats, incidents, evidence)
    ↓
Analyst (Read access to all, write access to threats and vulnerabilities)
    ↓
Viewer (Read-only access to all data)
```

**Permission Matrix**:

| Entity | SuperAdmin | Admin | Investigator | Analyst | Viewer |
|--------|------------|-------|--------------|---------|--------|
| Threats | CRUD | CRUD | CRUD | CRUD | R |
| Vulnerabilities | CRUD | CRUD | CRUD | CRUD | R |
| Incidents | CRUD | CRUD | CRUD | R | R |
| Evidence | CRUD | CRUD | CRUD | R | R |
| Hosts | CRUD | CRUD | R | R | R |
| Networks | CRUD | CRUD | R | R | R |
| Users | CRUD | R | - | - | - |
| Roles | CRUD | R | - | - | - |
| Permissions | CRUD | R | - | - | - |
| Settings | CRUD | CRUD | - | - | - |
| Audit Logs | R | R | R | - | - |
| Reports | CRUD | CRUD | R | R | R |

**Permissions**:
- Each role has a set of permissions
- Permissions can be assigned directly to users
- Permissions can be inherited from roles
- Permission checks are performed at the service layer

### 2. Data Classification

SOCpit implements a data classification system to protect sensitive information:

| Classification | Description | Access | Encryption | Retention |
|---------------|-------------|--------|-----------|-----------|
| Public | Non-sensitive data | All users | No | 1 year |
| Internal | Internal organization data | Authenticated users | No | 3 years |
| Confidential | Sensitive security data | Authorized users | Optional | 5 years |
| Restricted | Highly sensitive data | Specific users only | Required | 7 years |

**Evidence Classification**:
- Log files: Internal
- Screenshots: Confidential
- Network captures: Confidential
- Memory dumps: Restricted
- Configuration files: Confidential
- Audit logs: Restricted

### 3. Data Protection

#### Encryption
- **At Rest**: All sensitive data is encrypted (future implementation)
  - SQLite database encryption (SQLCipher or similar)
  - File encryption for evidence storage
  - Configuration file encryption
- **In Transit**: All network communications are encrypted
  - TLS 1.2+ for all network connections
  - Certificate validation
- **In Memory**: Sensitive data is protected in memory
  - Secure string handling
  - Memory clearing after use

#### Hashing
- **Passwords**: SHA256 with salt (future: bcrypt, Argon2)
- **Evidence Files**: SHA256 hash for integrity verification
- **API Keys**: SHA256 hash for storage

#### Data Validation
- All inputs are validated
- SQL injection prevention
- XSS prevention
- Path traversal prevention
- File type validation
- File size limits

### 4. Audit Logging

SOCpit maintains a comprehensive audit log of all security-relevant actions.

**Audit Log Structure**:
```csharp
public sealed class AuditLog
{
    public string Id { get; init; }           // Unique identifier
    public string Action { get; init; }        // Create, Read, Update, Delete, Login, etc.
    public string EntityType { get; init; }     // Threat, Vulnerability, User, etc.
    public string EntityId { get; init; }       // ID of the affected entity
    public string UserId { get; init; }         // User who performed the action
    public DateTimeOffset Timestamp { get; init; } // When the action occurred
    public string? Details { get; init; }       // Additional details
    public string? OldValue { get; init; }      // Previous value (for updates)
    public string? NewValue { get; init; }      // New value (for updates)
    public string? IpAddress { get; init; }     // IP address of the request
    public string? UserAgent { get; init; }     // User agent/browser
    public bool Success { get; init; }           // Whether the action succeeded
    public string? ErrorMessage { get; init; } // Error message if failed
}
```

**Audit Log Retention**:
- Minimum: 1 year (configurable)
- Recommended: 3-5 years
- Maximum: 7 years (for compliance)

**Audit Log Protection**:
- Immutable (cannot be modified after creation)
- Write-once, read-many (WORM) storage
- Signed to detect tampering
- Regular integrity checks

### 5. Network Security

#### Communication Security
- All network communications use TLS 1.2+
- Certificate validation is performed
- Weak cipher suites are disabled
- Perfect Forward Secrecy (PFS) is preferred

#### Import/Export Security
- File validation before import
- File size limits
- File type verification
- Malware scanning (future)
- Export encryption (future)

### 6. Application Security

#### Secure Coding Practices
- Input validation on all user inputs
- Output encoding to prevent XSS
- Parameterized queries to prevent SQL injection
- Secure error handling (no sensitive data in error messages)
- Secure logging (no sensitive data in logs)
- Memory management (clear sensitive data from memory)

#### Dependency Security
- Regular dependency updates
- Vulnerability scanning of dependencies
- Minimum version requirements
- Signed packages only

#### Secure Configuration
- Sensitive configuration is encrypted
- Configuration validation
- Secure defaults
- Environment-specific configuration

### 7. Session Management

**Session Properties**:
```csharp
public sealed class Session
{
    public string Id { get; init; }           // Unique session identifier
    public string UserId { get; init; }        // Associated user
    public string Token { get; init; }         // Session token
    public DateTimeOffset CreatedAt { get; init; } // Creation time
    public DateTimeOffset ExpiresAt { get; init; } // Expiration time
    public string IpAddress { get; init; }     // IP address
    public string UserAgent { get; init; }     // User agent
    public bool IsActive { get; init; }        // Active status
    public string? RefreshToken { get; init; } // Refresh token
}
```

**Session Security**:
- Session tokens are cryptographically random
- Session timeout: 30 minutes (configurable)
- Absolute session expiration: 8 hours (configurable)
- Session invalidation on logout
- Session invalidation on password change
- Concurrent session limits (configurable)
- Session activity monitoring

### 8. Password Policy

**Password Requirements**:
- Minimum length: 12 characters
- Maximum length: 128 characters
- Complexity requirements:
  - At least 1 uppercase letter
  - At least 1 lowercase letter
  - At least 1 digit
  - At least 1 special character
- Password history: Last 5 passwords cannot be reused
- Password expiration: 90 days (configurable)
- Account lockout after 5 failed attempts
- Lockout duration: 30 minutes (configurable)

**Password Storage**:
- SHA256 with unique salt (future: bcrypt, Argon2)
- Salt is unique per user
- Salt is stored separately from password hash

### 9. Evidence Security

**Evidence Handling**:
- All evidence is stored with integrity verification
- SHA256 hash is calculated and stored
- File size is verified
- File type is verified
- Evidence classification is required

**Evidence Access Control**:
- Access is controlled by classification level
- Evidence can be linked to specific incidents only
- Evidence access is logged
- Evidence download is logged

**Evidence Storage**:
- Evidence files are stored in a secure location
- File system permissions are set correctly
- Evidence can be encrypted at rest (future)
- Evidence can be stored externally (future)

### 10. Backup and Recovery

**Backup Strategy**:
- Automatic backups: Daily (configurable)
- Backup retention: 30 days (configurable)
- Backup encryption: AES-256 (future)
- Backup verification: Checksum validation
- Offsite backups: Recommended

**Disaster Recovery**:
- Point-in-time recovery capability
- Database backup and restore procedures
- Evidence file backup and restore
- Configuration backup and restore
- Recovery testing: Quarterly

## Security Implementation

### In SOCpit.Core

**Security Models**:
- `User.cs` - User identity and authentication
- `Permission.cs` - Permission definitions
- `Role.cs` - Role definitions
- `Session.cs` - Session management
- `AuditLog.cs` - Audit logging
- `SecurityControl.cs` - Security controls
- `ComplianceCheck.cs` - Compliance tracking

**Security Services**:
- `GraphValidator.cs` - Graph validation for security relationships

### In SOCpit.Application

**Security Services**:
- `UserService.cs` - User management with security checks
- `AuditService.cs` - Audit logging service
- `EvidenceService.cs` - Evidence management with security

**Security Interfaces**:
- `IUserRepository.cs` - User repository with security methods
- `IAuditLogRepository.cs` - Audit log repository

### In SOCpit.Storage

**Security Implementations**:
- `UserRepository.cs` - Secure user data access
- `AuditLogRepository.cs` - Secure audit log storage
- `SOCpitDbContext.cs` - Database security configuration

### In SOCpit.App

**Security Features**:
- Authentication UI
- Authorization checks
- Secure data binding
- Secure file handling

## Security Testing

### Security Test Cases

#### Authentication Tests
- [ ] Valid credentials login
- [ ] Invalid credentials login
- [ ] Account lockout after failed attempts
- [ ] Password complexity validation
- [ ] Password change
- [ ] Session creation
- [ ] Session expiration
- [ ] Session invalidation

#### Authorization Tests
- [ ] Role-based access control
- [ ] Permission-based access control
- [ ] Unauthorized access attempts
- [ ] Privilege escalation attempts
- [ ] Access to other users' data

#### Data Validation Tests
- [ ] SQL injection attempts
- [ ] XSS attempts
- [ ] Path traversal attempts
- [ ] File upload validation
- [ ] Input length validation
- [ ] Input type validation

#### Audit Logging Tests
- [ ] Action logging
- [ ] Error logging
- [ ] Audit log integrity
- [ ] Audit log immutability
- [ ] Audit log retention

#### Evidence Security Tests
- [ ] Evidence upload validation
- [ ] Evidence integrity verification
- [ ] Evidence classification enforcement
- [ ] Evidence access control
- [ ] Evidence download logging

### Security Testing Tools

**Static Analysis**:
- Roslyn Analyzers
- Security Code Scan
- SonarQube

**Dynamic Analysis**:
- OWASP ZAP
- Burp Suite
- Fiddler

**Dependency Scanning**:
- NuGet vulnerability scanning
- GitHub Dependabot
- Snyk

## Security Incident Response

### Incident Classification

| Severity | Description | Response Time | Resolution Time |
|----------|-------------|---------------|-----------------|
| Critical | Active exploitation, data breach | Immediate (15 min) | 4 hours |
| High | Security vulnerability, potential breach | 1 hour | 24 hours |
| Medium | Security issue, limited impact | 4 hours | 72 hours |
| Low | Security improvement, no immediate threat | 24 hours | 30 days |

### Incident Response Procedure

1. **Detection**: Security incident is detected
2. **Triage**: Initial assessment of severity and impact
3. **Containment**: Contain the incident to prevent further damage
4. **Eradication**: Remove the threat from the environment
5. **Recovery**: Restore normal operations
6. **Lessons Learned**: Document and learn from the incident

### Incident Response Team

**Roles**:
- **Incident Commander**: Overall coordination
- **Technical Lead**: Technical investigation and response
- **Communications Lead**: Internal and external communications
- **Legal/Compliance**: Legal and regulatory compliance
- **HR**: Human resources support

### Incident Response Tools

**Detection**:
- SIEM integration
- IDS/IPS alerts
- Log analysis
- User reports

**Investigation**:
- Forensic tools
- Log analysis tools
- Network analysis tools
- Memory analysis tools

**Communication**:
- Incident management system
- Email notifications
- SMS notifications
- Conference calls

## Security Compliance

### Compliance Standards

SOCpit is designed to support compliance with the following standards:

| Standard | Description | Coverage |
|----------|-------------|----------|
| ISO 27001 | Information Security Management | Partial |
| NIST CSF | Cybersecurity Framework | Partial |
| NIST 800-53 | Security and Privacy Controls | Partial |
| SOC 2 | Service Organization Controls | Partial |
| HIPAA | Health Insurance Portability and Accountability Act | Partial |
| PCI DSS | Payment Card Industry Data Security Standard | Partial |
| GDPR | General Data Protection Regulation | Partial |

### Compliance Features

**ISO 27001**:
- Access control (A.9)
- Asset management (A.8)
- Audit logging (A.12)
- Incident management (A.16)
- Information security policies (A.5)

**NIST CSF**:
- Identify: Asset management, risk assessment
- Protect: Access control, data protection
- Detect: Monitoring, anomaly detection
- Respond: Incident response
- Recover: Backup and recovery

**SOC 2**:
- Security: Access control, data protection
- Availability: Monitoring, redundancy
- Processing Integrity: Validation, error handling
- Confidentiality: Encryption, access control
- Privacy: Data classification, retention

### Compliance Reporting

SOCpit provides the following compliance reports:

1. **Access Control Report**: User access and permissions
2. **Asset Inventory Report**: All assets and their security status
3. **Audit Log Report**: All audit log entries
4. **Incident Report**: All security incidents
5. **Vulnerability Report**: All vulnerabilities and their status
6. **Compliance Check Report**: Compliance status for each control

## Security Best Practices

### For Administrators

1. **User Management**
   - Use strong, unique passwords
   - Assign minimum required permissions
   - Regularly review user accounts
   - Remove inactive accounts
   - Implement password expiration
   - Enable account lockout

2. **Data Management**
   - Classify all data appropriately
   - Implement data retention policies
   - Regularly backup data
   - Test backup restoration
   - Secure sensitive data

3. **Monitoring**
   - Enable audit logging
   - Monitor for suspicious activity
   - Set up alerts for critical events
   - Regularly review audit logs
   - Monitor system health

4. **Maintenance**
   - Keep software up to date
   - Apply security patches promptly
   - Monitor for vulnerabilities
   - Regularly test security controls

### For Users

1. **Authentication**
   - Use strong, unique passwords
   - Never share passwords
   - Change passwords regularly
   - Use multi-factor authentication when available
   - Lock your workstation when unattended

2. **Data Handling**
   - Classify data appropriately
   - Store sensitive data securely
   - Share data only with authorized users
   - Follow data retention policies
   - Report data breaches immediately

3. **Incident Reporting**
   - Report security incidents immediately
   - Provide accurate and complete information
   - Follow incident response procedures
   - Preserve evidence
   - Do not discuss incidents publicly

4. **System Usage**
   - Use only authorized software
   - Do not install unauthorized software
   - Report suspicious activity
   - Follow acceptable use policies
   - Keep software up to date

## Security Vulnerabilities

### Known Vulnerabilities

None known at this time.

### Vulnerability Reporting

If you discover a security vulnerability in SOCpit, please report it responsibly:

1. **Do not** disclose the vulnerability publicly
2. **Do not** exploit the vulnerability
3. **Do** report the vulnerability to the maintainers
4. **Do** provide detailed information about the vulnerability
5. **Do** provide steps to reproduce the vulnerability
6. **Do** provide potential impact of the vulnerability

### Vulnerability Response

1. **Acknowledgment**: Initial response within 24 hours
2. **Triage**: Assessment within 48 hours
3. **Fix**: Patch development based on severity
4. **Testing**: Verification of the fix
5. **Disclosure**: Coordinated disclosure with reporter

### Vulnerability Severity

| Severity | Description | Response Time | Disclosure Time |
|----------|-------------|---------------|-----------------|
| Critical | Remote code execution, privilege escalation | 24 hours | 7 days |
| High | Data exposure, denial of service | 48 hours | 14 days |
| Medium | Information disclosure, limited impact | 72 hours | 30 days |
| Low | Minor security issues | 7 days | Next release |

## Security Updates

### Version History

| Version | Date | Security Updates |
|---------|------|-----------------|
| 2026.10.09 | 2026-10-09 | Initial release |

### Security Update Policy

- Security updates are released as soon as possible
- Critical updates are released within 24 hours
- High priority updates are released within 48 hours
- Medium priority updates are released within 72 hours
- Low priority updates are released in the next regular release

### Security Update Distribution

- GitHub releases
- NuGet packages (future)
- Direct download from website (future)

## Security Contacts

For security-related questions or concerns:

- **Email**: security@aifirsts.com
- **GitHub**: https://github.com/aifirsts/SOCpit2026/security
- **Website**: https://aifirsts.com/socpit/security (future)

## Conclusion

Security is a top priority for SOCpit. This document outlines the comprehensive security policies, practices, and features implemented in SOCpit to protect your organization's sensitive security data. By following these security practices and regularly reviewing and updating security measures, SOCpit provides a secure platform for your Security Operations Center.

For the latest security information, please refer to the [Security Advisories](https://github.com/aifirsts/SOCpit2026/security/advisories) page on GitHub.
