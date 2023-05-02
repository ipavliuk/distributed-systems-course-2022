using ReplicatedLog.Master.Enums;
using System.Xml.Serialization;

namespace ReplicatedLog.Master.HeartBeat;

public interface IClusterHealthManager
{
    Dictionary<string, NodeStatus> GetSecondariesStatus();
    bool IsNodeAvailable(string url);

    void Start();
    void Stop();
}
