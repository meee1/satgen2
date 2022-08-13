using System;
using System.Collections.Generic;

namespace Racelogic.Core.Interfaces;

public interface IVlcPlayer : IDisposable
{
	string InstallationPath { get; set; }

	bool? GPUAccelerated { get; set; }

	List<string> FileList { get; set; }

	string File { get; set; }

	bool VideoLoaded { get; }

	double TimeInMilliSeconds { get; set; }

	bool IsPlaying { get; }

	bool IsPaused { get; }

	event VideoPositionChangedDelegate PositionChanged;

	event EventHandler EndOfVideoReached;

	event EventHandler MouseDown;

	event EventHandler MouseMove;

	event EventHandler MouseUp;

	event EventHandler DragEnter;

	event EventHandler DragLeave;

	event EventHandler DragOver;

	event EventHandler DragDrop;

	event EventHandler Playing;

	event EventHandler Paused;

	event EventHandler Stopped;

	void Play();

	void Pause();

	void Stop();
}
