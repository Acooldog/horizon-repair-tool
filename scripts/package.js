const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');

// 读取当前版本号
const packageJson = JSON.parse(fs.readFileSync('package.json', 'utf8'));
const version = packageJson.version;

// 源目录和目标目录
const sourceDir = 'bin/Release/net8.0-windows';
const releaseDir = 'release';
const zipName = `horizon-repair-tool-${version}.zip`;

// 创建 release 目录
if (!fs.existsSync(releaseDir)) {
  fs.mkdirSync(releaseDir);
}

// 使用 7z 压缩（如果可用），否则使用 PowerShell
try {
  // 尝试使用 7z
  execSync(`7z a -tzip ${path.join(releaseDir, zipName)} ${sourceDir}/*`);
  console.log(`使用 7z 创建压缩包: ${zipName}`);
} catch (error) {
  // 回退到 PowerShell
  try {
    execSync(`powershell Compress-Archive -Path "${sourceDir}/*" -DestinationPath "${path.join(releaseDir, zipName)}" -Force`);
    console.log(`使用 PowerShell 创建压缩包: ${zipName}`);
  } catch (psError) {
    console.error('压缩失败，请安装 7z 或确保 PowerShell 可用');
    process.exit(1);
  }
}

console.log(`✅ 打包完成: ${zipName}`);