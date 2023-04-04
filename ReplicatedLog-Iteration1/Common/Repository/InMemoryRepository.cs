using Common.Model;
using System.Linq;

namespace Common.Repository;

public class InMemoryRepository : IRepository
{
    private readonly List<Message> _inOrderBuffer = new List<Message>();
    private readonly List<Message> _outOfOrderBuffer = new List<Message>();
    private long _nextExpectedSequenceId = 1;
    private readonly object _lock = new object();

    public void Add(Message msg)
    {
        lock(_lock) // handle concurent requests - avoit race conditions in case of multiple request comming to append message
        {
            if (msg.SequenceId == _nextExpectedSequenceId)
            {
                _inOrderBuffer.Add(msg);
                _nextExpectedSequenceId++;

                MoveOutOfOrderMessagesToInOrderBuffer();
            }
            else if (msg.SequenceId > _nextExpectedSequenceId)
            {
                _outOfOrderBuffer.Add(msg);
            }
        }
        
    }
    private void MoveOutOfOrderMessagesToInOrderBuffer()
    {
        // Sort the out-of-order buffer by sequence ID
        _outOfOrderBuffer.Sort((a, b) => a.SequenceId.CompareTo(b.SequenceId));

        // Keep moving messages from the out-of-order buffer to the in-order buffer as long as they have the expected sequence ID
        while (_outOfOrderBuffer.Count > 0 && _outOfOrderBuffer[0].SequenceId == _nextExpectedSequenceId)
        {
            var message = _outOfOrderBuffer[0];
            _outOfOrderBuffer.RemoveAt(0);
            _inOrderBuffer.Add(message);
            _nextExpectedSequenceId++;
        }
    }

    public List<Message> GetAll()
    {
        return _inOrderBuffer;
    }

    public Message GetById(long id)
    {
        return _inOrderBuffer.Find(i => i.SequenceId == id);
    }

    public List<Message> GetOutOfOrderMessages()
    {
        return _outOfOrderBuffer;
    }
}
