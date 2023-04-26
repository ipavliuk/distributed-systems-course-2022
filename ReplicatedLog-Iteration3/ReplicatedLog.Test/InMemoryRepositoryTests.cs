using Common.Model;
using Common.Repository;

namespace ReplicatedLog.Secondary;


[TestClass]
public class InMemoryRepositoryTests
{
    private InMemoryRepository _repository;

    [TestInitialize]
    public void Setup()
    {
        _repository = new InMemoryRepository();
    }

    [TestMethod]
    public void Add_WhenMessageHasExpectedSequenceId_ShouldAddMessageToInOrderBuffer()
    {
        // Arrange
        var message = new Message(1, "Test message");

        // Act
        _repository.Add(message);

        // Assert
        var result = _repository.GetAll();
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(message, result[0]);
    }

    [TestMethod]
    public void Add_WhenMessageHasHigherSequenceId_ShouldAddMessageToOutOfOrderBuffer()
    {
        // Arrange
        var message = new Message(2, "Test message");

        // Act
        _repository.Add(message);

        // Assert
        var inOrderBuffer = _repository.GetAll();
        var outOfOrderBuffer = _repository.GetOutOfOrderMessages();
        Assert.AreEqual(0, inOrderBuffer.Count);
        Assert.AreEqual(1, outOfOrderBuffer.Count);
        Assert.AreEqual(message, outOfOrderBuffer[0]);
    }

    [TestMethod]
    public void Add_WhenMessagesAreAddedOutOfOrder_ShouldAddMessagesToOutOfOrderBufferAndThenInOrderBuffer()
    {
        // Arrange
        var message1 = new Message(2, "Test message 2");
        var message2 = new Message(3, "Test message 3");
        var message3 = new Message(1, "Test message 1");

        // Act
        _repository.Add(message1);
        _repository.Add(message2);
        _repository.Add(message3);

        // Assert
        var inOrderBuffer = _repository.GetAll();
        var outOfOrderBuffer = _repository.GetOutOfOrderMessages();
        Assert.AreEqual(3, inOrderBuffer.Count);
        Assert.AreEqual(0, outOfOrderBuffer.Count);
        Assert.AreEqual(message3, inOrderBuffer[0]);
        Assert.AreEqual(message1, inOrderBuffer[1]);
        Assert.AreEqual(message2, inOrderBuffer[2]);
    }

    [TestMethod]
    public void GetById_WhenMessageExistsInInOrderBuffer_ShouldReturnMessage()
    {
        // Arrange
        var message = new Message(1, "Test message");
        _repository.Add(message);

        // Act
        var result = _repository.GetById(1);

        // Assert
        Assert.AreEqual(message, result);
    }

    [TestMethod]
    public void GetById_WhenMessageDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var message = new Message(1, "Test message");
        _repository.Add(message);

        // Act
        var result = _repository.GetById(2);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void TestConcurrentAddMessages()
    {
        // Create a new InMemoryRepository instance
        var repository = new InMemoryRepository();

        // Create a list of messages to add to the repository
        var messages = new List<Message>()
        {
            new Message(1, "Message 1"),
            new Message(3, "Message 3"),
            new Message(2, "Message 2"),
            new Message(5, "Message 5"),
            new Message(10, "Message 10"),
            new Message(4, "Message 4"),
            new Message(7, "Message 7"),
            new Message(8, "Message 8"),
            new Message(9, "Message 9"),
            new Message(6, "Message 6")
        };


        // Create a list of threads to add the messages to the repository
        var threads = new List<Thread>();
        foreach (var message in messages)
        {
            threads.Add(new Thread(() => repository.Add(message)));
        }

        // Start the threads and wait for them to finish
        foreach (var thread in threads)
        {
            thread.Start();
        }
        foreach (var thread in threads)
        {
            thread.Join();
        }

        // Verify that all messages have been added to the repository in the expected order
        var expectedMessages = new List<Message>()
        {
            new Message(1, "Message 1"),
            new Message(2, "Message 2"),
            new Message(3, "Message 3"),
            new Message(4, "Message 4"),
            new Message(5, "Message 5"),
            new Message(6, "Message 6"),
            new Message(7, "Message 7"),
            new Message(8, "Message 8"),
            new Message(9, "Message 9"),
            new Message(10, "Message 10")
        };
        var actualMessages = repository.GetAll();
        Console.WriteLine($"actualMessages: { string.Join("; ", actualMessages.Select(m => $"{m.SequenceId} - {m.Msg}"))}");
        CollectionAssert.AreEqual(expectedMessages, actualMessages);
    }

}
