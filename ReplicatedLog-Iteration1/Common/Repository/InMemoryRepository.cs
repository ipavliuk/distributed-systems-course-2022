using Common.Model;
using System.Linq;

namespace Common.Repository;

public class InMemoryRepository : IRepository
{
    private readonly List<Message> _storage = new List<Message>();
    public void Add(Message msg)
    {
        _storage.Add(msg);
    }

    public List<Message> GetAll()
    {
        return _storage;
    }

    public Message GetById(long id)
    {
        return _storage.Find(i => i.Id == id);
    }
}
