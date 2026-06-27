const fs = require('fs');
const path = 'c:\\Users\\W1338\\Desktop\\Industrial-Toolkit\\frontend\\software-engineer.html';
let content = fs.readFileSync(path, 'utf8');

// 1. Delete CSS monitor styles
const cssOld = `        }
        
        .monitor-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 20px;
        }
        
        .monitor-card {
            background: white;
            padding: 25px;
            border-radius: 12px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.05);
            transition: transform 0.3s, box-shadow 0.3s;
        }
        
        .monitor-card:hover {
            transform: translateY(-5px);
            box-shadow: 0 8px 20px rgba(0,0,0,0.1);
        }
        
        .monitor-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 15px;
        }
        
        .monitor-icon { font-size: 24px; }
        .monitor-title { font-size: 14px; color: #666; }
        
        .monitor-value {
            font-size: 36px;
            font-weight: bold;
            color: #2c3e50;
            margin-bottom: 10px;
        }
        
        .monitor-bar {
            height: 8px;
            background: #eee;
            border-radius: 4px;
            overflow: hidden;
        }
        
        .monitor-fill {
            height: 100%;
            border-radius: 4px;
            transition: width 0.5s;
        }
        
        .monitor-fill.cpu { background: linear-gradient(90deg, #3498db, #2980b9); }
        .monitor-fill.memory { background: linear-gradient(90deg, #e74c3c, #c0392b); }
        .monitor-fill.disk { background: linear-gradient(90deg, #27ae60, #1e8449); }
        
        .monitor-info { font-size: 12px; color: #888; margin-top: 8px; }
        
        .thread-container {`;

const cssNew = `        }
        
        .thread-container {`;

if (content.includes(cssOld)) {
    content = content.replace(cssOld, cssNew);
    console.log('CSS replacement OK');
} else {
    console.log('CSS replacement FAILED');
}

// 2. Delete HTML section
const htmlOld = `            <section class="section">
                <h2 class="section-title">📊 系统监控</h2>
                <div class="monitor-grid">
                    <div class="monitor-card">
                        <div class="monitor-header">
                            <span class="monitor-icon">🖥️</span>
                            <span class="monitor-title">CPU使用率</span>
                        </div>
                        <div class="monitor-value" id="cpuValue">0%</div>
                        <div class="monitor-bar">
                            <div class="monitor-fill cpu" id="cpuBar"></div>
                        </div>
                        <div class="monitor-info" id="cpuInfo">CPU核心数: 0</div>
                    </div>
                    
                    <div class="monitor-card">
                        <div class="monitor-header">
                            <span class="monitor-icon">💾</span>
                            <span class="monitor-title">内存使用率</span>
                        </div>
                        <div class="monitor-value" id="memoryValue">0%</div>
                        <div class="monitor-bar">
                            <div class="monitor-fill memory" id="memoryBar"></div>
                        </div>
                        <div class="monitor-info" id="memoryInfo">总内存: 0 GB</div>
                    </div>
                    
                    <div class="monitor-card">
                        <div class="monitor-header">
                            <span class="monitor-icon">📁</span>
                            <span class="monitor-title">硬盘使用率</span>
                        </div>
                        <div class="monitor-value" id="diskValue">0%</div>
                        <div class="monitor-bar">
                            <div class="monitor-fill disk" id="diskBar"></div>
                        </div>
                        <div class="monitor-info" id="diskInfo">总容量: 0 GB</div>
                    </div>
                    
                    <div class="monitor-card">
                        <div class="monitor-header">
                            <span class="monitor-icon">⚡</span>
                            <span class="monitor-title">系统负载</span>
                        </div>
                        <div class="monitor-value" id="loadValue">0.00</div>
                        <div class="monitor-bar">
                            <div class="monitor-fill cpu" id="loadBar"></div>
                        </div>
                        <div class="monitor-info" id="loadInfo">正常</div>
                    </div>
                </div>
                <button class="btn btn-primary" onclick="refreshSystemInfo()" style="margin-top: 20px;">刷新系统信息</button>
            </section>
            
            <section class="section">`;

const htmlNew = `            <section class="section">`;

if (content.includes(htmlOld)) {
    content = content.replace(htmlOld, htmlNew);
    console.log('HTML replacement OK');
} else {
    console.log('HTML replacement FAILED');
}

// 3. Delete JS function
const jsOld = `        function refreshSystemInfo() {
            fetch('/api/system/info')
                .then(response => response.json())
                .then(data => {
                    document.getElementById('cpuValue').textContent = (data.cpuUsage || Math.floor(Math.random() * 50 + 10)) + '%';
                    document.getElementById('cpuBar').style.width = (data.cpuUsage || Math.floor(Math.random() * 50 + 10)) + '%';
                    document.getElementById('cpuInfo').textContent = 'CPU核心数: ' + (data.cpuCores || 4);
                    
                    document.getElementById('memoryValue').textContent = (data.memoryUsage || Math.floor(Math.random() * 40 + 20)) + '%';
                    document.getElementById('memoryBar').style.width = (data.memoryUsage || Math.floor(Math.random() * 40 + 20)) + '%';
                    document.getElementById('memoryInfo').textContent = '总内存: ' + (data.totalMemory || 16) + ' GB';
                    
                    document.getElementById('diskValue').textContent = (data.diskUsage || Math.floor(Math.random() * 30 + 40)) + '%';
                    document.getElementById('diskBar').style.width = (data.diskUsage || Math.floor(Math.random() * 30 + 40)) + '%';
                    document.getElementById('diskInfo').textContent = '总容量: ' + (data.totalDisk || 512) + ' GB';
                    
                    const load = (Math.random() * 2).toFixed(2);
                    document.getElementById('loadValue').textContent = load;
                    document.getElementById('loadBar').style.width = Math.min(parseFloat(load) * 30, 100) + '%';
                    document.getElementById('loadInfo').textContent = parseFloat(load) > 1.5 ? '较高' : '正常';
                })
                .catch(() => {
                    document.getElementById('cpuValue').textContent = '35%';
                    document.getElementById('cpuBar').style.width = '35%';
                    document.getElementById('cpuInfo').textContent = 'CPU核心数: 8';
                    
                    document.getElementById('memoryValue').textContent = '42%';
                    document.getElementById('memoryBar').style.width = '42%';
                    document.getElementById('memoryInfo').textContent = '总内存: 16 GB';
                    
                    document.getElementById('diskValue').textContent = '58%';
                    document.getElementById('diskBar').style.width = '58%';
                    document.getElementById('diskInfo').textContent = '总容量: 512 GB';
                    
                    document.getElementById('loadValue').textContent = '0.85';
                    document.getElementById('loadBar').style.width = '25%';
                    document.getElementById('loadInfo').textContent = '正常';
                });
        }
        
        refreshSystemInfo();
        
        function calculateThreads() {`;

const jsNew = `        function calculateThreads() {`;

if (content.includes(jsOld)) {
    content = content.replace(jsOld, jsNew);
    console.log('JS replacement OK');
} else {
    console.log('JS replacement FAILED');
}

fs.writeFileSync(path, content, 'utf8');
console.log('File saved.');
