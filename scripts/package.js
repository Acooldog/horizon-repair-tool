const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');

// 读取当前版本号
const packageJson = JSON.parse(fs.readFileSync('package.json', 'utf8'));
const version = packageJson.version;

// 目录配置
const sourceDir = 'bin/Release/net8.0-windows';
const releaseDir = 'release';
// const zipName = `horizon-repair-tool-${version}.zip`;
const zipName = `horizon-repair-tool.zip`;

// 1. 清理旧版本文件
console.log('🧹 清理旧版本文件...');
if (fs.existsSync(releaseDir)) {
    const files = fs.readdirSync(releaseDir);
    files.forEach(file => {
        if (file.endsWith('.zip') && file.startsWith('horizon-repair-tool-')) {
            const filePath = path.join(releaseDir, file);
            fs.unlinkSync(filePath);
            console.log(`🗑️  删除旧文件: ${file}`);
        }
    });
} else {
    // 创建 release 目录（如果不存在）
    fs.mkdirSync(releaseDir, { recursive: true });
}

// 2. 检查源文件是否存在
if (!fs.existsSync(sourceDir)) {
    console.error(`❌ 错误: 源目录不存在: ${sourceDir}`);
    console.log('💡 请先运行: dotnet build -c Release');
    process.exit(1);
}

// 3. 构建压缩包
console.log(`📦 创建压缩包: ${zipName}`);
try {
    // 尝试使用 7z
    execSync(`7z a -tzip "${path.join(releaseDir, zipName)}" "${sourceDir}/*"`, { stdio: 'inherit' });
    console.log(`✅ 使用 7z 创建压缩包: ${zipName}`);
} catch (error) {
    // 回退到 PowerShell
    try {
        execSync(`powershell -Command "Compress-Archive -Path '${sourceDir}/*' -DestinationPath '${path.join(releaseDir, zipName)}' -Force"`, { stdio: 'inherit' });
        console.log(`✅ 使用 PowerShell 创建压缩包: ${zipName}`);
    } catch (psError) {
        console.error('❌ 压缩失败，请安装 7z 或确保 PowerShell 可用');
        process.exit(1);
    }
}

// 4. 验证压缩包
if (fs.existsSync(path.join(releaseDir, zipName))) {
    const stats = fs.statSync(path.join(releaseDir, zipName));
    console.log(`🎉 打包完成: ${zipName} (${(stats.size / 1024 / 1024).toFixed(2)} MB)`);
} else {
    console.error('❌ 压缩包创建失败');
    process.exit(1);
}