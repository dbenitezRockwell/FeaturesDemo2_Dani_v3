#region Using directives
using FTOptix.Core;
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.HMIProject;
using FTOptix.OPCUAServer;
using FTOptix.UI;
using FTOptix.NativeUI;
using FTOptix.CoreBase;
using FTOptix.NetLogic;
using FTOptix.RAEtherNetIP;
using FTOptix.CommunicationDriver;
using FTOptix.System;
using FTOptix.SerialPort;
#endregion

public class IdleTimeoutLogic : BaseNetLogic
{
    public override void Start()
    {
        duration = LogicObject.GetVariable("Duration");
        if (duration == null)
            throw new CoreConfigurationException("Unable to find Duration variable");

        enabled = LogicObject.GetVariable("Enabled");
        if (enabled == null)
            throw new CoreConfigurationException("Unable to find Enabled variable");

        onTimeout = (MethodInvocation)LogicObject.Get("OnTimeout");
        if (onTimeout == null)
            throw new CoreConfigurationException("Unable to find OnTimeout method invocation");

        uiSession = Session as UISession;
        if (uiSession == null)
            throw new CoreConfigurationException("Idle Timeout logic must be placed inside a UI object");

        enabled.VariableChange += Enabled_VariableChange;
        duration.VariableChange += Duration_VariableChange;

        uiSession.OnIdleTimeout += UiSession_OnIdleTimeout;
        uiSession.IdleTimeoutEnabled = enabled.Value;
        uiSession.IdleTimeoutDuration = TimeSpan.FromMilliseconds(duration.Value);
    }

    private void Enabled_VariableChange(object sender, VariableChangeEventArgs e)
    {
        uiSession.IdleTimeoutEnabled = e.NewValue;
    }

    private void Duration_VariableChange(object sender, VariableChangeEventArgs e)
    {
        uiSession.IdleTimeoutDuration = TimeSpan.FromMilliseconds(e.NewValue);
    }

    private void UiSession_OnIdleTimeout(object sender, IdleTimeoutEvent e)
    {
        onTimeout.Invoke();
    }

    public override void Stop()
    {
        if (uiSession != null)
            uiSession.OnIdleTimeout -= UiSession_OnIdleTimeout;
        if (enabled != null)
            enabled.VariableChange -= Enabled_VariableChange;
        if (duration != null)
            duration.VariableChange -= Duration_VariableChange;
    }

    private UISession uiSession;
    private IUAVariable duration;
    private IUAVariable enabled;
    private MethodInvocation onTimeout;
}
