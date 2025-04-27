using CC.ChatSupport.Application;
using CC.ChatSupport.Domain;
using System.Threading.Channels;

namespace CC.ChatSupport.Tests;

public class ChatQueueServiceTests
{
    [Fact]
    public async Task EnqueueChatAsync_ShouldWriteChatSessionToChannel()
    {
        //--Arrange
        var channel = Channel.CreateUnbounded<ChatSession>();
        var service = new ChatQueueService(channel);

        //--Act
        var result = await service.EnqueueChatAsync();

        //--Assert
        Assert.NotNull(result);  

        //--Read the session from the channel
        var success = channel.Reader.TryRead(out var sessionFromChannel);

        Assert.True(success);                   
        Assert.Equal(result.Id, sessionFromChannel?.Id); 
    }
}
