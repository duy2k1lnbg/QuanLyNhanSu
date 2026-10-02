import { useState, useRef, useEffect, useCallback } from 'react';

export function useAmbientAudio() {
  const [isPlaying, setIsPlaying] = useState<boolean>(false);
  const audioCtxRef = useRef<AudioContext | null>(null);
  const gainNodeRef = useRef<GainNode | null>(null);
  const noiseSourceRef = useRef<AudioNode | null>(null);

  // Initialize or resume tranquil synthesized woodland/rain ambient sound via Web Audio API
  // This guarantees smooth audio without external network dependencies or 404 errors!
  const startAmbientSound = useCallback(() => {
    try {
      const AudioContextClass = window.AudioContext || (window as any).webkitAudioContext;
      if (!AudioContextClass) return;

      if (!audioCtxRef.current) {
        audioCtxRef.current = new AudioContextClass();
      }

      const ctx = audioCtxRef.current;
      if (ctx.state === 'suspended') {
        ctx.resume();
      }

      // Create gentle filtered noise resembling soft woodland wind and distant leaves
      const bufferSize = ctx.sampleRate * 2;
      const buffer = ctx.createBuffer(1, bufferSize, ctx.sampleRate);
      const data = buffer.getChannelData(0);
      let lastOut = 0.0;

      // Pink noise algorithm for soothing gentle sound
      for (let i = 0; i < bufferSize; i++) {
        const white = Math.random() * 2 - 1;
        lastOut = (lastOut + 0.02 * white) / 1.02;
        data[i] = lastOut * 1.5;
      }

      const noise = ctx.createBufferSource();
      noise.buffer = buffer;
      noise.loop = true;

      // Lowpass filter to simulate gentle forest canopy breeze
      const filter = ctx.createBiquadFilter();
      filter.type = 'lowpass';
      filter.frequency.setValueAtTime(380, ctx.currentTime);

      const gain = ctx.createGain();
      gain.gain.setValueAtTime(0.0001, ctx.currentTime);
      // Gentle fade in over 800ms
      gain.gain.exponentialRampToValueAtTime(0.12, ctx.currentTime + 0.8);

      noise.connect(filter);
      filter.connect(gain);
      gain.connect(ctx.destination);

      noise.start();

      noiseSourceRef.current = noise;
      gainNodeRef.current = gain;
      setIsPlaying(true);
    } catch {
      // Graceful fallback if AudioContext is blocked by browser policy
      setIsPlaying(false);
    }
  }, []);

  const stopAmbientSound = useCallback(() => {
    try {
      if (gainNodeRef.current && audioCtxRef.current) {
        const ctx = audioCtxRef.current;
        // Fade out
        gainNodeRef.current.gain.linearRampToValueAtTime(0.0001, ctx.currentTime + 0.5);
        setTimeout(() => {
          if (noiseSourceRef.current) {
            (noiseSourceRef.current as any).stop?.();
            noiseSourceRef.current.disconnect();
            noiseSourceRef.current = null;
          }
          setIsPlaying(false);
        }, 550);
      } else {
        setIsPlaying(false);
      }
    } catch {
      setIsPlaying(false);
    }
  }, []);

  const toggleAudio = useCallback(() => {
    if (isPlaying) {
      stopAmbientSound();
    } else {
      startAmbientSound();
    }
  }, [isPlaying, startAmbientSound, stopAmbientSound]);

  // Handle visibility change (pause on background tab)
  useEffect(() => {
    const handleVisibility = () => {
      if (document.hidden && isPlaying) {
        if (audioCtxRef.current && audioCtxRef.current.state === 'running') {
          audioCtxRef.current.suspend();
        }
      } else if (!document.hidden && isPlaying) {
        if (audioCtxRef.current && audioCtxRef.current.state === 'suspended') {
          audioCtxRef.current.resume();
        }
      }
    };

    document.addEventListener('visibilitychange', handleVisibility);
    return () => {
      document.removeEventListener('visibilitychange', handleVisibility);
    };
  }, [isPlaying]);

  // Cleanup on unmount (e.g. when logging in)
  useEffect(() => {
    return () => {
      if (noiseSourceRef.current) {
        try {
          (noiseSourceRef.current as any).stop?.();
          noiseSourceRef.current.disconnect();
        } catch {
          // ignore
        }
      }
      if (audioCtxRef.current) {
        try {
          audioCtxRef.current.close();
        } catch {
          // ignore
        }
      }
    };
  }, []);

  return { isPlaying, toggleAudio };
}
