using System;
using System.Text;
using UnityEngine;

public class LogSession : IDisposable
{

    public enum Channel
    { 
        Log,
        Warning,
        Error,
        Assert,
        Exception
    }

    private bool disposed = false;
    private Channel channel = Channel.Log;
    private StringBuilder messageBuilder = new StringBuilder();

    // Function not really too useful, but added to complete the set
    public bool ChannelLog(bool allowDeescallation = false)
    {
        return ChangeChannel(Channel.Log, allowDeescallation);
    }

    public bool ChannelWarning(bool allowDeescallation = false)
    {
        return ChangeChannel(Channel.Warning, allowDeescallation);
    }

    public bool ChannelError(bool allowDeescallation = false)
    {
        return ChangeChannel(Channel.Error, allowDeescallation);
    }

    public bool ChannelAssert(bool allowDeescallation = false)
    {
        return ChangeChannel(Channel.Assert, allowDeescallation);
    }

    public bool ChannelException(bool allowDeescallation = false)
    {
        return ChangeChannel(Channel.Exception, allowDeescallation);
    }

    private bool ChangeChannel(Channel c, bool allowDeescallation)
    {
        if(!allowDeescallation && c < channel)
                return false;
        
        this.channel = c;
        return true;
    }

    public void Append(string msg)
    {
        messageBuilder.Append(msg);
    }

    public void AppendLine(string msg)
    {
        messageBuilder.AppendLine(msg);
    }

    public void AppendErrorLine(string msg)
    { 
        messageBuilder.AppendLine(msg);
        ChannelError(false);
    }

    void IDisposable.Dispose()
    { 
        if(disposed)
            return;
        
        disposed = true;
        FlushLog();
    }

    ~LogSession()
    { 
        IDisposable id = this;
        id.Dispose();
    }

    private void FlushLog()
    {
        switch(this.channel)
        { 
            case Channel.Log:
                Debug.Log(this.messageBuilder.ToString());
                break;
            case Channel.Warning:
                Debug.LogWarning(this.messageBuilder.ToString());
                break;
            case Channel.Error:
                Debug.LogError(this.messageBuilder.ToString());
                break;
            case Channel.Assert:
                Debug.Assert(false, this.messageBuilder.ToString());
                break;
            case Channel.Exception:
                throw new Exception(this.messageBuilder.ToString());
            default:
                throw new NotImplementedException();
        }
    }
}
