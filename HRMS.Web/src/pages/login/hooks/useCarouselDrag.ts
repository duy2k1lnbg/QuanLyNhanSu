import { useState, useRef, useCallback } from 'react';

interface UseCarouselDragOptions {
  onNext: () => void;
  onPrev: () => void;
  onDragStart?: () => void;
  onDragEnd?: () => void;
}

export function useCarouselDrag({
  onNext,
  onPrev,
  onDragStart,
  onDragEnd,
}: UseCarouselDragOptions) {
  const [dragOffset, setDragOffset] = useState<number>(0);
  const [isDragging, setIsDragging] = useState<boolean>(false);

  const startXRef = useRef<number>(0);
  const startYRef = useRef<number>(0);
  const startTimeRef = useRef<number>(0);
  const isLockedHorizontalRef = useRef<boolean | null>(null);
  const pointerIdRef = useRef<number | null>(null);
  const containerWidthRef = useRef<number>(window.innerWidth || 1200);

  const handlePointerDown = useCallback(
    (e: React.PointerEvent<HTMLDivElement>) => {
      // Ignore right clicks or clicks on buttons/links
      if (e.button !== 0) return;
      const target = e.target as HTMLElement;
      if (target.closest('button, a, input, select, textarea, .interactive-control')) {
        return;
      }

      startXRef.current = e.clientX;
      startYRef.current = e.clientY;
      startTimeRef.current = performance.now();
      isLockedHorizontalRef.current = null;
      pointerIdRef.current = e.pointerId;

      const currentTarget = e.currentTarget;
      if (currentTarget) {
        containerWidthRef.current = currentTarget.clientWidth || window.innerWidth;
      }
    },
    []
  );

  const handlePointerMove = useCallback(
    (e: React.PointerEvent<HTMLDivElement>) => {
      if (pointerIdRef.current === null || pointerIdRef.current !== e.pointerId) return;

      const deltaX = e.clientX - startXRef.current;
      const deltaY = e.clientY - startYRef.current;

      // Axis lock detection threshold of ~8px
      if (isLockedHorizontalRef.current === null) {
        if (Math.abs(deltaX) > 8 || Math.abs(deltaY) > 8) {
          if (Math.abs(deltaX) > Math.abs(deltaY)) {
            isLockedHorizontalRef.current = true;
            setIsDragging(true);
            onDragStart?.();
            try {
              (e.currentTarget as HTMLElement).setPointerCapture(e.pointerId);
            } catch {
              // Ignore if browser restricts capture
            }
          } else {
            isLockedHorizontalRef.current = false;
          }
        }
      }

      if (isLockedHorizontalRef.current === true) {
        // Apply slight resistance
        const dampenedOffset = deltaX * 0.85;
        setDragOffset(dampenedOffset);
      }
    },
    [onDragStart]
  );

  const settleDrag = useCallback(
    (e: React.PointerEvent<HTMLDivElement>) => {
      if (pointerIdRef.current === null || pointerIdRef.current !== e.pointerId) return;

      if (isLockedHorizontalRef.current === true) {
        const deltaX = e.clientX - startXRef.current;
        const duration = Math.max(1, performance.now() - startTimeRef.current);
        const velocity = Math.abs(deltaX) / duration;
        const threshold = Math.min(100, containerWidthRef.current * 0.2);

        if (deltaX < -threshold || (deltaX < -30 && velocity > 0.4)) {
          // Dragged left -> Next slide
          onNext();
        } else if (deltaX > threshold || (deltaX > 30 && velocity > 0.4)) {
          // Dragged right -> Prev slide
          onPrev();
        }

        try {
          (e.currentTarget as HTMLElement).releasePointerCapture(e.pointerId);
        } catch {
          // Ignore
        }
      }

      pointerIdRef.current = null;
      isLockedHorizontalRef.current = null;
      setIsDragging(false);
      setDragOffset(0);
      onDragEnd?.();
    },
    [onNext, onPrev, onDragEnd]
  );

  return {
    dragOffset,
    isDragging,
    dragProps: {
      onPointerDown: handlePointerDown,
      onPointerMove: handlePointerMove,
      onPointerUp: settleDrag,
      onPointerCancel: settleDrag,
      style: {
        touchAction: 'pan-y' as const,
        cursor: isDragging ? 'grabbing' : 'grab',
      },
    },
  };
}
