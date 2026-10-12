// In-app notifications do not need OS permission or a remote push token.
export function startForegroundRefresh(
  state: {currentState: string | null; addEventListener: (event: 'change', listener: (state: string) => void) => {remove: () => void}},
  refresh: () => void,
  schedule: (callback: () => void, delay: number) => any = setInterval,
  cancel: (id: any) => void = clearInterval,
): () => void {
  let timer: any;
  const stop = () => { if (timer !== undefined) {cancel(timer); timer=undefined;} };
  const update = (next: string | null) => {
    stop();
    if (next === 'active') {refresh(); timer=schedule(refresh,30000);}
  };
  const listener=state.addEventListener('change', update);
  update(state.currentState);
  return () => {stop(); listener.remove();};
}
