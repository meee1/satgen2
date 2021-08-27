using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Racelogic.Gnss
{
	public class ConvolutionalEnumerator : IEnumerator<byte>, IEnumerator, IDisposable
	{
		private readonly IEnumerator<byte> inputEnumerator;

		private int input;

		private int output;

		private bool g1 = true;

		private readonly bool negateG2;

		private int inputIndex;

		private readonly int captureIndex;

		private int? capturedState;

		private int register;

		private readonly int[] g1Taps = new int[4] { 5, 4, 3, 0 };

		private readonly int[] g2Taps = new int[4] { 4, 3, 1, 0 };

		private const int newBitIndex = 5;

		public const int ConstraintLength = 7;

		public const int ByteRatio = 2;

		private bool isDisposed;

		public byte Current
		{
			[DebuggerStepThrough]
			get
			{
				return (byte)output;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerStepThrough]
			get
			{
				return (byte)output;
			}
		}

		public int? CapturedState
		{
			[DebuggerStepThrough]
			get
			{
				return capturedState;
			}
		}

		public ConvolutionalEnumerator(IEnumerable<byte> inputSequence, in int registerState = 0, in int captureIndex = 0, ConvolutionalEncoderOptions options = ConvolutionalEncoderOptions.None)
		{
			if (inputSequence == null)
			{
				throw new ArgumentNullException("inputSequence");
			}
			register = registerState;
			this.captureIndex = captureIndex;
			negateG2 = options == ConvolutionalEncoderOptions.NegateG2;
			inputEnumerator = inputSequence.GetEnumerator();
		}

		public bool MoveNext()
		{
			if (g1)
			{
				if (!inputEnumerator.MoveNext())
				{
					return false;
				}
				input = inputEnumerator.Current;
				output = input;
				int[] array = g1Taps;
				foreach (int num in array)
				{
					output ^= (register >> num) & 1;
				}
			}
			else
			{
				output = input;
				int[] array = g2Taps;
				foreach (int num2 in array)
				{
					output ^= (register >> num2) & 1;
				}
				if (negateG2)
				{
					output ^= 1;
				}
				register >>= 1;
				register |= input << 5;
				inputIndex++;
				if (inputIndex == captureIndex)
				{
					capturedState = register;
				}
			}
			g1 = !g1;
			return true;
		}

		public void Reset()
		{
			throw new NotSupportedException();
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!isDisposed)
			{
				isDisposed = true;
				if (disposing)
				{
					inputEnumerator.Dispose();
				}
			}
		}
	}
}
