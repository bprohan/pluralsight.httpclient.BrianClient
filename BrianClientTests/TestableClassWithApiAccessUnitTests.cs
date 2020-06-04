using Moq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq.Protected;
using Xunit;

namespace BrianClientTests
{
    public class TestableClassWithApiAccessUnitTests
    {
        [Fact]
        public void GetMovie_On401Response_ThrowsCorrectException()
        {
            var httpClient = new HttpClient(new Return401UnauthorizedResponseHandler());

            var sut = new TestableClassWithApiAccess(httpClient);

            var cs = new CancellationTokenSource();

            _ = Assert.ThrowsAsync<UnauthorizedApiAccessException>(
                () => sut.GetMovie(cs.Token)
            );
        }


        // below is cleaner than writing custom handlers for each test use case.
        [Fact]
        public void GetMovie_On401Response_MustThrowUnauthorizedApiAccessException_WithMoq()
        {
            var unauthorizedResponseHttpMessageHandlerMock = new Mock<HttpMessageHandler>();

            unauthorizedResponseHttpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                ).ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.Unauthorized
                });

            var httpClient = new HttpClient(unauthorizedResponseHttpMessageHandlerMock.Object);

            var testableClass = new TestableClassWithApiAccess(httpClient);

            var cancellationTokenSource = new CancellationTokenSource();

            Assert.ThrowsAsync<UnauthorizedApiAccessException>(
                () => testableClass.GetMovie(cancellationTokenSource.Token));
        }
    }
}
