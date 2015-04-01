using System;
using System.Collections.Generic;
using System.Threading;

namespace Nito
{
    /// <summary>
    /// A cancellation token wrapper that may represent multiple linked cancellation tokens. Instances of this type should always be disposed.
    /// </summary>
    public sealed class NormalizedCancellationToken : IDisposable
    {
        /// <summary>
        /// The <see cref="CancellationTokenSource"/>, if any. If this is not <c>null</c>, then <see cref="_token"/> is <c>_cts.Token</c>.
        /// </summary>
        private readonly CancellationTokenSource _cts;

        /// <summary>
        /// The <see cref="Token"/>. If <see cref="_cts"/> is not <c>null</c>, then this is <c>_cts.Token</c>.
        /// </summary>
        private readonly CancellationToken _token;

        /// <summary>
        /// Creates a normalized cancellation token that can never be canceled.
        /// </summary>
        public NormalizedCancellationToken()
        {
        }

        /// <summary>
        /// Creates a normalized cancellation token from a <see cref="CancellationTokenSource"/>. <see cref="Token"/> is set to the <see cref="CancellationTokenSource.Token"/> property of <paramref name="cts"/>.
        /// </summary>
        /// <param name="cts">The source for this token.</param>
        public NormalizedCancellationToken(CancellationTokenSource cts)
        {
            _cts = cts;
            _token = cts.Token;
        }

        /// <summary>
        /// Creates a normalized cancellation token from a <see cref="CancellationToken"/>. <see cref="Token"/> is set to <paramref name="token"/>.
        /// </summary>
        /// <param name="token">The source for this token.</param>
        public NormalizedCancellationToken(CancellationToken token)
        {
            _token = token;
        }

        /// <summary>
        /// Releases any resources used by this normalized cancellation token.
        /// </summary>
        public void Dispose()
        {
            if (_cts != null)
                _cts.Dispose();
        }

        /// <summary>
        /// Gets the <see cref="CancellationToken"/> for this normalized cancellation token.
        /// </summary>
        public CancellationToken Token { get { return _token; } }

        /// <summary>
        /// Creates a cancellation token that is canceled after the due time.
        /// </summary>
        /// <param name="dueTime">The due time after which to cancel the token.</param>
        /// <returns>A cancellation token that is canceled after the due time.</returns>
        public static NormalizedCancellationToken Timeout(TimeSpan dueTime)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(dueTime);
            return new NormalizedCancellationToken(cts);
        }

        /// <summary>
        /// Creates a cancellation token that is canceled after the due time.
        /// </summary>
        /// <param name="dueTime">The due time after which to cancel the token.</param>
        /// <returns>A cancellation token that is canceled after the due time.</returns>
        public static NormalizedCancellationToken Timeout(int dueTime)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(dueTime);
            return new NormalizedCancellationToken(cts);
        }

        /// <summary>
        /// Reduces a set of cancellation tokens by removing any cancellation tokens that cannot be canceled. If any tokens are already canceled, the returned token will be canceled.
        /// </summary>
        /// <param name="cancellationTokens">The cancellation tokens to reduce.</param>
        public static NormalizedCancellationToken Normalize(params CancellationToken[] cancellationTokens)
        {
            return Normalize((IEnumerable<CancellationToken>)cancellationTokens);
        }

        /// <summary>
        /// Reduces a set of cancellation tokens by removing any cancellation tokens that cannot be canceled. If any tokens are already canceled, the returned token will be canceled.
        /// </summary>
        /// <param name="cancellationTokens">The cancellation tokens to reduce.</param>
        public static NormalizedCancellationToken Normalize(IEnumerable<CancellationToken> cancellationTokens)
        {
            var tokens = new List<CancellationToken>(CancelableTokens(cancellationTokens));
            if (tokens.Count == 0)
                return new NormalizedCancellationToken();
            if (tokens.Count == 1)
                return new NormalizedCancellationToken(tokens[0]);
            var alreadyCanceled = FindCanceledToken(tokens);
            if (alreadyCanceled.IsCancellationRequested)
                return new NormalizedCancellationToken(alreadyCanceled);
            var tokenArray = new CancellationToken[tokens.Count];
            ((ICollection<CancellationToken>)tokenArray).CopyTo(tokenArray, 0);
            return new NormalizedCancellationToken(CancellationTokenSource.CreateLinkedTokenSource(tokenArray));
        }

        private static IEnumerable<CancellationToken> CancelableTokens(IEnumerable<CancellationToken> tokens)
        {
            foreach (var token in tokens)
                if (token.CanBeCanceled)
                    yield return token;
        }

        private static CancellationToken FindCanceledToken(IEnumerable<CancellationToken> tokens)
        {
            foreach (var token in tokens)
                if (token.IsCancellationRequested)
                    return token;
            return CancellationToken.None;
        }
    }
}