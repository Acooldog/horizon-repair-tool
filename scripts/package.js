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

// 1. 自动运行 dotnet build
console.log('🚀 开始构建 .NET 项目...');
try {
    console.log('📋 恢复依赖...');
    execSync('dotnet restore', { stdio: 'inherit' });
    
    console.log('🔨 编译项目...');
    execSync('dotnet build -c Release', { stdio: 'inherit' });
    
    console.log('✅ .NET 项目构建完成！');
} catch (buildError) {
    console.error('❌ .NET 项目构建失败:');
    console.error(buildError.message);
    process.exit(1);
}

// 2. 清理旧版本文件
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

// 3. 检查源文件是否存在
if (!fs.existsSync(sourceDir)) {
    console.error(`❌ 错误: 源目录不存在: ${sourceDir}`);
    console.error('💡 即使构建成功，输出目录也未创建，请检查项目配置');
    process.exit(1);
}

// 检查输出文件是否存在
const exeFiles = fs.readdirSync(sourceDir).filter(file => file.endsWith('.exe'));
if (exeFiles.length === 0) {
    console.error(`❌ 错误: 在 ${sourceDir} 中未找到可执行文件`);
    console.error('💡 请检查 .csproj 文件配置是否正确');
    process.exit(1);
}

console.log(`📁 找到输出文件: ${exeFiles.join(', ')}`);

// 4. 构建压缩包
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

// 5. 验证压缩包
if (fs.existsSync(path.join(releaseDir, zipName))) {
    const stats = fs.statSync(path.join(releaseDir, zipName));
    console.log(`🎉 打包完成: ${zipName} (${(stats.size / 1024 / 1024).toFixed(2)} MB)`);
    
    // 显示压缩包内容
    console.log('📋 压缩包内容:');
    try {
        execSync(`7z l "${path.join(releaseDir, zipName)}" | findstr "Date Time" -A 10`, { stdio: 'inherit' });
    } catch (e) {
        // 如果 7z 不可用，使用 PowerShell 列出内容
        try {
            execSync(`powershell -Command "Get-ChildItem '${sourceDir}' | Select-Object Name, Length"`, { stdio: 'inherit' });
        } catch (psError) {
            console.log('📄 包含文件: 所有编译输出文件');
        }
    }
} else {
    console.error('❌ 压缩包创建失败');
    process.exit(1);
}

console.log('✨ 所有步骤完成！');