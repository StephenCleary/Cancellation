using System;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using Xunit;
using Nito;

namespace UnitTests
{
    public class NomalizedCancellationTokenUnitTests
    {
        [Fact]
        public void Normalize_ManyUncancelableTokens_ReturnsTokenThatCannotCancel()
        {
            var result = NormalizedCancellationToken.Normalize(CancellationToken.None, CancellationToken.None, CancellationToken.None);

            Assert.False(result.Token.CanBeCanceled);
            Assert.Equal(CancellationToken.None, result.Token);
        }

        [Fact]
        public void Normalize_OneCancelableTokenWithManyUncancelableTokens_ReturnsCancelableToken()
        {
            var cts = new CancellationTokenSource();

            var result = NormalizedCancellationToken.Normalize(CancellationToken.None, cts.Token, CancellationToken.None, CancellationToken.None);

            Assert.True(result.Token.CanBeCanceled);
            Assert.Equal(cts.Token, result.Token);
        }

        [Fact]
        public void Normalize_ManyCancelableTokens_ReturnsNewCancelableToken()
        {
            var cts1 = new CancellationTokenSource();
            var cts2 = new CancellationTokenSource();

            var result = NormalizedCancellationToken.Normalize(cts1.Token, cts2.Token);

            Assert.True(result.Token.CanBeCanceled);
            Assert.NotEqual(cts1.Token, result.Token);
            Assert.NotEqual(cts2.Token, result.Token);
        }
    }
}
