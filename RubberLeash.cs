using System;

public class RubberLeash : MVRScript
{
    public override void Init()
    {
        try
        {
            SuperController.LogMessage($"{nameof(RubberLeash)} initialized");
        }
        catch (Exception e)
        {
            SuperController.LogError($"{nameof(RubberLeash)}.{nameof(Init)}: {e}");
        }
    }

    public void OnEnable()
    {
        try
        {
            SuperController.LogMessage($"{nameof(RubberLeash)} enabled");
        }
        catch (Exception e)
        {
            SuperController.LogError($"{nameof(RubberLeash)}.{nameof(Init)}: {e}");
        }
    }

    public void OnDisable()
    {
        try
        {
            SuperController.LogMessage($"{nameof(RubberLeash)} disabled");
        }
        catch (Exception e)
        {
            SuperController.LogError($"{nameof(RubberLeash)}.{nameof(Init)}: {e}");
        }
    }

    public void OnDestroy()
    {
        try
        {
            SuperController.LogMessage($"{nameof(RubberLeash)} destroyed");
        }
        catch (Exception e)
        {
            SuperController.LogError($"{nameof(RubberLeash)}.{nameof(Init)}: {e}");
        }
    }
}
