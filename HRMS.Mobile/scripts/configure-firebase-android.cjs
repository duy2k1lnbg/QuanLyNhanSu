const fs=require('node:fs');
const path=require('node:path');
const root=path.resolve(__dirname,'..');
const plugin=require(path.join(root,'node_modules/@expo/config-plugins/build/android/GoogleServices.js'));
const config=JSON.parse(fs.readFileSync(path.join(root,'app.json'),'utf8')).expo;
(async()=>{
  const project=path.join(root,'android/build.gradle');
  const app=path.join(root,'android/app/build.gradle');
  fs.writeFileSync(project,plugin.setClassPath(config,fs.readFileSync(project,'utf8')));
  fs.writeFileSync(app,plugin.applyPlugin(config,fs.readFileSync(app,'utf8')));
  await plugin.setGoogleServicesFile(config,root);
  console.log('Applied SDK 57 Firebase Gradle plugin and copied verified Android configuration.');
})().catch(error=>{console.error(error.message);process.exitCode=1;});
