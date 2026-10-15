using System;
using System.Collections.Generic;
using SOCpit.Core;
using SOCpit.TestSupport;
using Xunit;

namespace SOCpit.Core.Tests;

/// <summary>
/// ADR-V2-SEC-001: the conservative security policy must never turn
/// UNKNOWN / STALE / MISSING / REVOKED / SUPERSEDED state into PASS.
/// </summary>
public class SecurityPolicyTests
{
    private static readonly DateTimeOffset Now = TestData.Now;

    private static Security Compute(Control control, params ControlEvidence[] evidence)
        => SecurityPolicy.ComputeNodeSecurity(control.TargetId, new[] { control }, evidence, Now);

    private static Security ComputeMany(IReadOnlyList<Control> controls, IReadOnlyList<ControlEvidence> evidence)
        => SecurityPolicy.ComputeNodeSecurity(controls[0].TargetId, controls, evidence, Now);

    [Fact]
    public void ComputeNodeSecurity_NoControls_ReturnsUnknown()
    {
        var status = SecurityPolicy.ComputeNodeSecurity("node-1", Array.Empty<Control>(), Array.Empty<ControlEvidence>(), Now);
        Assert.Equal(Security.Unknown, status);
    }

    [Fact]
    public void ComputeNodeSecurity_NullArguments_ReturnsUnknown()
    {
        Assert.Equal(Security.Unknown, SecurityPolicy.ComputeNodeSecurity("", Array.Empty<Control>(), Array.Empty<ControlEvidence>(), Now));
        Assert.Equal(Security.Unknown, SecurityPolicy.ComputeNodeSecurity("node-1", null!, Array.Empty<ControlEvidence>(), Now));
        Assert.Equal(Security.Unknown, SecurityPolicy.ComputeNodeSecurity("node-1", Array.Empty<Control>(), null!, Now));
    }

    [Fact]
    public void ComputeNodeSecurity_RequiredControlWithoutEvidence_ReturnsUnknown()
    {
        var control = TestData.NewControl(targetId: "node-1", required: true);
        Assert.Equal(Security.Unknown, Compute(control));
    }

    [Fact]
    public void ComputeNodeSecurity_OptionalControlWithoutEvidence_DoesNotBlockPass()
    {
        var control = TestData.NewControl(targetId: "node-1", required: false);
        Assert.Equal(Security.Pass, Compute(control));
    }

    [Fact]
    public void ComputeNodeSecurity_FreshPassEvidence_ReturnsPass()
    {
        var control = TestData.NewControl(targetId: "node-1");
        var evidence = TestData.NewControlEvidence(controlId: control.Id, result: EvidenceResult.Pass, observedAt: Now.AddMinutes(-10), validUntil: Now.AddMinutes(50));
        Assert.Equal(Security.Pass, Compute(control, evidence));
    }

    [Fact]
    public void ComputeNodeSecurity_FailEvidence_ReturnsFail()
    {
        var control = TestData.NewControl(targetId: "node-1");
        var evidence = TestData.NewControlEvidence(controlId: control.Id, result: EvidenceResult.Fail);
        Assert.Equal(Security.Fail, Compute(control, evidence));
    }

    [Fact]
    public void ComputeNodeSecurity_BlockedEvidence_ReturnsBlocked()
    {
        var control = TestData.NewControl(targetId: "node-1");
        var evidence = TestData.NewControlEvidence(controlId: control.Id, result: EvidenceResult.Blocked);
        Assert.Equal(Security.Blocked, Compute(control, evidence));
    }

    [Fact]
    public void ComputeNodeSecurity_WarningEvidence_ReturnsUnknown()
    {
        var control = TestData.NewControl(targetId: "node-1");
        var evidence = TestData.NewControlEvidence(controlId: control.Id, result: EvidenceResult.Warning);
        Assert.Equal(Security.Unknown, Compute(control, evidence));
    }

    [Fact]
    public void ComputeNodeSecurity_FailWinsOverBlocked()
    {
        var blockedControl = TestData.NewControl(id: "ctrl-blocked", targetId: "node-1");
        var failingControl = TestData.NewControl(id: "ctrl-fail", targetId: "node-1");

        var blocked = TestData.NewControlEvidence(id: "cev-blocked", controlId: "ctrl-blocked", result: EvidenceResult.Blocked);
        var fail = TestData.NewControlEvidence(id: "cev-fail", controlId: "ctrl-fail", result: EvidenceResult.Fail);

        Assert.Equal(Security.Fail, SecurityPolicy.ComputeNodeSecurity(
            "node-1",
            new[] { blockedControl, failingControl },
            new[] { blocked, fail },
            Now));
    }

    [Fact]
    public void ComputeNodeSecurity_NewestEvidenceWinsForTheSameControl()
    {
        var control = TestData.NewControl(targetId: "node-1");
        var olderFail = TestData.NewControlEvidence(id: "cev-old", controlId: control.Id, result: EvidenceResult.Fail, observedAt: Now.AddMinutes(-30), validUntil: Now.AddMinutes(30));
        var newerPass = TestData.NewControlEvidence(id: "cev-new", controlId: control.Id, result: EvidenceResult.Pass, observedAt: Now.AddMinutes(-5), validUntil: Now.AddMinutes(55));

        Assert.Equal(Security.Pass, Compute(control, olderFail, newerPass));
    }

    [Fact]
    public void ComputeNodeSecurity_StaleEvidence_ReturnsUnknown()
    {
        var control = TestData.NewControl(targetId: "node-1");
        var stale = TestData.NewControlEvidence(controlId: control.Id, observedAt: Now.AddHours(-5), validUntil: Now.AddMinutes(-1));
        Assert.Equal(Security.Unknown, Compute(control, stale));
    }

    [Fact]
    public void ComputeNodeSecurity_EvidenceWithExactExpiry_IsNotValid()
    {
        var control = TestData.NewControl(targetId: "node-1");
        var expired = TestData.NewControlEvidence(controlId: control.Id, observedAt: Now.AddHours(-1), validUntil: Now);
        Assert.Equal(Security.Unknown, Compute(control, expired));
    }

    [Fact]
    public void ComputeNodeSecurity_FutureEvidence_IsNotValid()
    {
        var control = TestData.NewControl(targetId: "node-1");
        var future = TestData.NewControlEvidence(controlId: control.Id, observedAt: Now.AddMinutes(5), validUntil: Now.AddHours(1));
        Assert.Equal(Security.Unknown, Compute(control, future));
    }

    [Fact]
    public void ComputeNodeSecurity_RevokedEvidence_ReturnsUnknown()
    {
        var control = TestData.NewControl(targetId: "node-1");
        var revoked = TestData.NewControlEvidence(controlId: control.Id, result: EvidenceResult.Pass, revoked: true);
        Assert.Equal(Security.Unknown, Compute(control, revoked));
    }

    [Fact]
    public void ComputeNodeSecurity_SupersededEvidence_IsIgnored()
    {
        var control = TestData.NewControl(targetId: "node-1");
        var old = TestData.NewControlEvidence(id: "cev-old", controlId: control.Id, result: EvidenceResult.Pass, observedAt: Now.AddMinutes(-40), validUntil: Now.AddMinutes(20));
        var newer = TestData.NewControlEvidence(id: "cev-new", controlId: control.Id, result: EvidenceResult.Pass, observedAt: Now.AddMinutes(-5), validUntil: Now.AddMinutes(55), supersedes: new List<string> { "cev-old" });

        Assert.Equal(Security.Pass, Compute(control, old, newer));
    }

    [Fact]
    public void ComputeNodeSecurity_NewestEvidenceSupersedeFailOverridesOlderPass()
    {
        var control = TestData.NewControl(targetId: "node-1");
        var oldPass = TestData.NewControlEvidence(id: "cev-old", controlId: control.Id, result: EvidenceResult.Pass, observedAt: Now.AddMinutes(-40), validUntil: Now.AddMinutes(60));
        var newFail = TestData.NewControlEvidence(id: "cev-new", controlId: control.Id, result: EvidenceResult.Fail, observedAt: Now.AddMinutes(-5), validUntil: Now.AddMinutes(60), supersedes: new List<string> { "cev-old" });

        Assert.Equal(Security.Fail, Compute(control, oldPass, newFail));
    }

    [Fact]
    public void ComputeNodeSecurity_RevokedSupersedeDoesNotInvalidateEvidence()
    {
        var control = TestData.NewControl(targetId: "node-1");
        var pass = TestData.NewControlEvidence(id: "cev-pass", controlId: control.Id, result: EvidenceResult.Pass, observedAt: Now.AddMinutes(-40), validUntil: Now.AddMinutes(60));
        var revokedSupersede = TestData.NewControlEvidence(id: "cev-bad", controlId: control.Id, result: EvidenceResult.Fail, observedAt: Now.AddMinutes(-5), validUntil: Now.AddMinutes(60), revoked: true, supersedes: new List<string> { "cev-pass" });

        Assert.Equal(Security.Pass, Compute(control, pass, revokedSupersede));
    }

    [Fact]
    public void ComputeNodeSecurity_AllControlsNotApplicableWithRationale_ReturnsNotApplicable()
    {
        var control = TestData.NewControl(targetId: "node-1", applicability: ControlApplicability.NotApplicable);
        Assert.Equal(Security.NotApplicable, Compute(control));
    }

    [Fact]
    public void ComputeNodeSecurity_EvidenceFromAnotherControl_IsIgnored()
    {
        var control = TestData.NewControl(targetId: "node-1");
        var foreignEvidence = TestData.NewControlEvidence(controlId: "ctrl-other", result: EvidenceResult.Pass);
        Assert.Equal(Security.Unknown, Compute(control, foreignEvidence));
    }

    [Fact]
    public void ComputeNodeSecurity_ControlsOfOtherNodes_AreIgnored()
    {
        var control = TestData.NewControl(targetId: "node-2");
        var evidence = TestData.NewControlEvidence(controlId: control.Id, result: EvidenceResult.Pass);

        var status = SecurityPolicy.ComputeNodeSecurity("node-1", new[] { control }, new[] { evidence }, Now);
        Assert.Equal(Security.Unknown, status);
    }

    [Fact]
    public void ComputeNodeSecurity_MultipleControlsFailsWhenAnyRequiredControlIsUnknown()
    {
        var passing = TestData.NewControl(id: "ctrl-pass", targetId: "node-1", required: true);
        var missing = TestData.NewControl(id: "ctrl-missing", targetId: "node-1", required: true);
        var evidence = TestData.NewControlEvidence(controlId: "ctrl-pass", result: EvidenceResult.Pass);

        Assert.Equal(Security.Unknown, ComputeMany(
            new[] { passing, missing },
            new[] { evidence }));
    }

    [Fact]
    public void ComputeNodeSecurity_MultipleControlsPassWhenAllHaveFreshPassEvidence()
    {
        var first = TestData.NewControl(id: "ctrl-1", targetId: "node-1");
        var second = TestData.NewControl(id: "ctrl-2", targetId: "node-1");
        var evidenceOne = TestData.NewControlEvidence(controlId: "ctrl-1", result: EvidenceResult.Pass);
        var evidenceTwo = TestData.NewControlEvidence(controlId: "ctrl-2", result: EvidenceResult.Pass);

        Assert.Equal(Security.Pass, ComputeMany(
            new[] { first, second },
            new[] { evidenceOne, evidenceTwo }));
    }

    [Fact]
    public void ValidateSupersedes_NullList_Fails()
    {
        Assert.False(SecurityPolicy.ValidateSupersedes(null!, out var error));
        Assert.NotNull(error);
    }

    [Fact]
    public void ValidateSupersedes_ValidChain_Succeeds()
    {
        var first = TestData.NewControlEvidence(id: "cev-1", controlId: "ctrl-1");
        var second = TestData.NewControlEvidence(id: "cev-2", controlId: "ctrl-1", supersedes: new List<string> { "cev-1" });
        var third = TestData.NewControlEvidence(id: "cev-3", controlId: "ctrl-1", supersedes: new List<string> { "cev-2" });

        Assert.True(SecurityPolicy.ValidateSupersedes(new[] { first, second, third }, out var error));
        Assert.Null(error);
    }

    [Fact]
    public void ValidateSupersedes_SelfReference_Fails()
    {
        var evidence = TestData.NewControlEvidence(id: "cev-1", controlId: "ctrl-1", supersedes: new List<string> { "cev-1" });

        Assert.False(SecurityPolicy.ValidateSupersedes(new[] { evidence }, out var error));
        Assert.Contains("itself", error);
    }

    [Fact]
    public void ValidateSupersedes_MissingTarget_Fails()
    {
        var evidence = TestData.NewControlEvidence(id: "cev-1", controlId: "ctrl-1", supersedes: new List<string> { "cev-ghost" });

        Assert.False(SecurityPolicy.ValidateSupersedes(new[] { evidence }, out var error));
        Assert.Contains("missing", error);
    }

    [Fact]
    public void ValidateSupersedes_CrossControlSupersede_Fails()
    {
        var first = TestData.NewControlEvidence(id: "cev-1", controlId: "ctrl-1");
        var second = TestData.NewControlEvidence(id: "cev-2", controlId: "ctrl-2", supersedes: new List<string> { "cev-1" });

        Assert.False(SecurityPolicy.ValidateSupersedes(new[] { first, second }, out var error));
        Assert.Contains("different control", error);
    }

    [Fact]
    public void ValidateSupersedes_DuplicateIds_Fails()
    {
        var first = TestData.NewControlEvidence(id: "cev-1", controlId: "ctrl-1");
        var duplicate = TestData.NewControlEvidence(id: "cev-1", controlId: "ctrl-1");

        Assert.False(SecurityPolicy.ValidateSupersedes(new[] { first, duplicate }, out var error));
        Assert.Contains("Duplicate", error);
    }

    [Fact]
    public void ValidateSupersedes_Cycle_Fails()
    {
        var first = TestData.NewControlEvidence(id: "cev-1", controlId: "ctrl-1", supersedes: new List<string> { "cev-2" });
        var second = TestData.NewControlEvidence(id: "cev-2", controlId: "ctrl-1", supersedes: new List<string> { "cev-1" });

        Assert.False(SecurityPolicy.ValidateSupersedes(new[] { first, second }, out var error));
        Assert.Contains("cycle", error);
    }

    [Fact]
    public void ValidateSupersedes_NullItem_Fails()
    {
        Assert.False(SecurityPolicy.ValidateSupersedes(new ControlEvidence[] { null! }, out var error));
        Assert.NotNull(error);
    }

    [Fact]
    public void SecurityPolicy_ExposesDocumentedLimits()
    {
        Assert.Equal(32, SecurityPolicy.MaxJsonDepth);
        Assert.Equal(16L * 1024 * 1024, SecurityPolicy.MaxInputBytes);
    }
}
