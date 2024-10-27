using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace ClubberAI.ServiceDefaults
{
	public class BalanceService : IDisposable
	{
		private readonly Dictionary<string, ReplaySubject<BigInteger>> _walletBalances = new();
		private readonly CompositeDisposable _disposables = new();
		private bool _disposed;

		public BalanceService()
		{
		}

		public void UpdateBalance(string wallet, BigInteger balance)
		{
			ThrowIfDisposed();
			if (!_walletBalances.TryGetValue(wallet, out var subject))
			{
				subject = new ReplaySubject<BigInteger>(1);
				_walletBalances[wallet] = subject;
				_disposables.Add(subject); // Add to composite disposable
			}
			subject.OnNext(balance);
		}

		public IObservable<BigInteger> ListenToBalanceChanges(string wallet)
		{
			ThrowIfDisposed();
			if (!_walletBalances.TryGetValue(wallet, out var subject))
			{
				subject = new ReplaySubject<BigInteger>(1);
				_walletBalances[wallet] = subject;
				_disposables.Add(subject); // Add to composite disposable
			}
			return subject.AsObservable();
		}

		private void ThrowIfDisposed()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(nameof(BalanceService));
			}
		}

		public void Dispose()
		{
			if (!_disposed)
			{
				_disposables.Dispose();
				_walletBalances.Clear();
				_disposed = true;
			}
		}
	}
}
