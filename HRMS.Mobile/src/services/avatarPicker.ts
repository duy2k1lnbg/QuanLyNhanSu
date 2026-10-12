import * as ImagePicker from 'expo-image-picker';

export type AvatarPickerAction = 'library' | 'camera';

export function avatarFromResult(result: ImagePicker.ImagePickerResult | ImagePicker.ImagePickerErrorResult | null) {
  if (!result) return null;
  if ('code' in result) throw new Error('mobileReview.avatarReadFailed');
  if (result.canceled) return null;
  const base64 = result.assets?.[0]?.base64;
  if (!base64) throw new Error('mobileReview.avatarReadFailed');
  const padding = base64.endsWith('==') ? 2 : base64.endsWith('=') ? 1 : 0;
  if (base64.length * 3 / 4 - padding > 5 * 1024 * 1024) throw new Error('mobileReview.avatarTooLarge');
  // Expo's base64 result contains JPEG bytes, irrespective of the source MIME type.
  return {base64, uri: `data:image/jpeg;base64,${base64}`};
}

export async function pickAvatar(action: AvatarPickerAction) {
  const options: ImagePicker.ImagePickerOptions = {
    mediaTypes: ['images'], allowsEditing: true, aspect: [1, 1], quality: 0.7, base64: true,
  };
  if (action === 'camera') {
    const permission = await ImagePicker.requestCameraPermissionsAsync();
    if (!permission.granted) throw new Error('mobileReview.cameraPermission');
    return avatarFromResult(await ImagePicker.launchCameraAsync(options));
  }
  // The system photo picker grants access to the selected photo; broad library
  // permission must not prevent selection on modern Android/iOS.
  return avatarFromResult(await ImagePicker.launchImageLibraryAsync(options));
}
