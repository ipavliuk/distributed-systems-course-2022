using Common.Model;

namespace Common.Repository;

public interface IRepository
{
    void Add(Message obj);
    Message GetById(long id);
    List<Message> GetAll();
}
