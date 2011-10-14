﻿using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.Pins.Config
{
	[ComVisible(false)]
	public class ConfigPin<T> : Pin<T>, IDiffSpread<T>
	{
		private readonly IPluginConfig FPluginConfig;
		
		public ConfigPin(IPluginHost host, IPluginConfig pluginConfig, IIOStream<T> stream)
			: base(host, pluginConfig, stream)
		{
			FPluginConfig = pluginConfig;
		}
		
		public ConfigPin(IPluginHost host, IPluginConfig pluginConfig, IInStream<T> inStream, IOutStream<T> outStream)
			: this(host, pluginConfig, new ConfigIOStream<T>(inStream, outStream))
		{
		}
		
		public event SpreadChangedEventHander<T> Changed;
		
		protected SpreadChangedEventHander FChanged;
		event SpreadChangedEventHander IDiffSpread.Changed
		{
			add
			{
				FChanged += value;
			}
			remove
			{
				FChanged -= value;
			}
		}
		
		protected virtual void OnChanged()
		{
			if (Changed != null)
				Changed(this);
			if (FChanged != null)
				FChanged(this);
		}
		
		public bool IsChanged
		{
			get
			{
				return FPluginConfig.PinIsChanged;
			}
		}
		
		public override sealed void Update()
		{
			if (IsChanged)
			{
				FStream.Sync();
				OnChanged();
			}
			
			OnUpdated();
		}
		
		protected override void DisposeManaged()
		{
			Changed = null;
			base.DisposeManaged();
		}
	}
	
	// In and out streams use the same data store. Simply delegate.
	class ConfigIOStream<T> : IIOStream<T>
	{
		private readonly IInStream<T> FInStream;
		private readonly IOutStream<T> FOutStream;
		
		public ConfigIOStream(IInStream<T> inStream, IOutStream<T> outStream)
		{
			FInStream = inStream;
			FOutStream = outStream;
		}
		
		public int ReadPosition 
		{
			get 
			{
				return FInStream.ReadPosition;
			}
			set
			{
				FInStream.ReadPosition = value;
			}
		}
		
		public int Length
		{
			get
			{
				return FInStream.Length;
			}
			set
			{
				FOutStream.Length = value;
			}
		}
		
		public bool Eof 
		{
			get
			{
				return ReadPosition >= Length || WritePosition >= Length;
			}
		}
		
		public int WritePosition 
		{
			get 
			{
				return FOutStream.WritePosition;
			}
			set 
			{
				FOutStream.WritePosition = value;
			}
		}
		
		public T Read(int stepSize)
		{
			return FInStream.Read(stepSize);
		}
		
		public int Read(T[] buffer, int index, int length, int stepSize)
		{
			return FInStream.Read(buffer, index, length, stepSize);
		}
		
		public void ReadCyclic(T[] buffer, int index, int length, int stepSize)
		{
			StreamUtils.ReadCyclic(FInStream, buffer, index, length, stepSize);
		}
		
		public void Sync()
		{
			FInStream.Sync();
		}
		
		public void Reset()
		{
			FInStream.Reset();
			FOutStream.Reset();
		}
		
		public object Clone()
		{
			throw new NotImplementedException();
		}
		
		public void Write(T value, int stepSize)
		{
			FOutStream.Write(value, stepSize);
		}
		
		public int Write(T[] buffer, int index, int length, int stepSize)
		{
			return FOutStream.Write(buffer, index, length, stepSize);
		}
		
		public void Flush()
		{
			FOutStream.Flush();
		}
	}
}
