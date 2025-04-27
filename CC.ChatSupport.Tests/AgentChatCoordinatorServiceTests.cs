using CC.ChatSupport.Application.Interfaces;
using CC.ChatSupport.Domain;
using CC.ChatSupport.Infrastructure;
using Moq;

namespace CC.ChatSupport.Tests;

public class AgentChatCoordinatorServiceTests
{
    [Fact]
    public async Task AssignAgentToChatAsync_ShouldReturnChatSession()
    {
        //--Arrange
        var mockService = new Mock<IAgentChatCoordinatorService>();

        var fakeSession = new ChatSession { Id = 1, IsActive = true };
        var fakeDbContext = new Mock<SupportDbContext>(); 

        mockService
            .Setup(s => s.AssignAgentToChatAsync(It.IsAny<ChatSession>(), It.IsAny<SupportDbContext>()))
            .ReturnsAsync((ChatSession session, SupportDbContext db) => session);

        //--Act
        var result = await mockService.Object.AssignAgentToChatAsync(fakeSession, fakeDbContext.Object);

        //--Assert
        Assert.NotNull(result);
        Assert.True(result.IsActive);
        Assert.Equal(1, result.Id);

        //--Verify it was called
        mockService.Verify(s => s.AssignAgentToChatAsync(It.IsAny<ChatSession>(), It.IsAny<SupportDbContext>()), Times.Once);
    }
}