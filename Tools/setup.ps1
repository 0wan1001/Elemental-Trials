# ClaudeKit 사전 준비 (Windows)
# 필요한 도구가 없으면 설치하고, Git 설정을 한 번에 끝낸다. 여러 번 실행해도 안전하다.
#
# 사용법 (저장소 루트에서):
#   powershell -ExecutionPolicy Bypass -File Tools/setup.ps1
#   powershell -ExecutionPolicy Bypass -File Tools/setup.ps1 -ProjectPath "C:\경로\Unity프로젝트"
#
# 하는 일
#   1. Git(+Git LFS), GitHub CLI(gh), Unity CLI 설치 (없을 때만)
#   2. git lfs install
#   3. Unity Smart Merge(UnityYAMLMerge)를 git 병합 도구로 등록
#   4. -ProjectPath를 주면 그 Unity 프로젝트에 com.unity.pipeline 패키지 추가
#   5. GitHub 로그인 상태 확인 (로그인은 직접: gh auth login)

param([string]$ProjectPath)

function Test-Command($name) {
    return [bool](Get-Command $name -ErrorAction SilentlyContinue)
}

function Update-SessionPath {
    # 방금 설치한 도구를 이 창에서 바로 쓰기 위해 PATH를 다시 읽는다
    $env:Path = [Environment]::GetEnvironmentVariable('Path', 'Machine') + ';' +
                [Environment]::GetEnvironmentVariable('Path', 'User')
}

function Install-WithWinget($id, $label) {
    Write-Host "[설치] $label"
    winget install --id $id -e --silent --accept-source-agreements --accept-package-agreements
}

if (-not (Test-Command 'winget')) {
    Write-Host "[오류] winget이 없습니다. Microsoft Store에서 '앱 설치 관리자'를 설치한 뒤 다시 실행하세요."
    exit 1
}

# 1. 도구 설치
if (Test-Command 'git') { Write-Host '[있음] Git' } else { Install-WithWinget 'Git.Git' 'Git (+ Git LFS)' }
if (Test-Command 'gh') { Write-Host '[있음] gh' } else { Install-WithWinget 'GitHub.cli' 'GitHub CLI (gh)' }
Update-SessionPath

if (Test-Command 'unity') {
    Write-Host '[있음] Unity CLI'
} else {
    Write-Host '[설치] Unity CLI'
    Invoke-RestMethod https://unity.com/install.ps1 | Invoke-Expression
    Update-SessionPath
}

# 2. Git LFS
git lfs install | Out-Null
Write-Host '[설정] Git LFS'

# 3. Unity Smart Merge 등록
#    프로젝트의 Unity 버전이 설치돼 있으면 그 버전의 도구를, 없으면 설치된 것 중 가장 최신 버전을 쓴다.
#    Unity Hub 기본 경로와, Hub에서 바꾼 설치 경로(secondaryInstallPath)를 모두 찾는다.
function Get-UnityVersionKey($name) {
    if ($name -match '^(\d+)\.(\d+)\.(\d+)') { return [version]"$($Matches[1]).$($Matches[2]).$($Matches[3])" }
    return [version]'0.0.0'
}

$editorRoots = @('C:\Program Files\Unity\Hub\Editor')
$secondary = Join-Path $env:APPDATA 'UnityHub\secondaryInstallPath.json'
if (Test-Path $secondary) {
    $secondaryPath = Get-Content $secondary -Raw | ConvertFrom-Json
    if ($secondaryPath) { $editorRoots += $secondaryPath }
}

$candidates = foreach ($root in $editorRoots) {
    Get-ChildItem $root -Directory -ErrorAction SilentlyContinue | ForEach-Object {
        $exe = Join-Path $_.FullName 'Editor\Data\Tools\UnityYAMLMerge.exe'
        if (Test-Path $exe) { [pscustomobject]@{ Version = $_.Name; Path = $exe } }
    }
}

$wantedVersion = $null
if ($ProjectPath) {
    $versionFile = Join-Path $ProjectPath 'ProjectSettings\ProjectVersion.txt'
    if (Test-Path $versionFile) {
        $match = Select-String -Path $versionFile -Pattern 'm_EditorVersion:\s*(\S+)'
        if ($match) { $wantedVersion = $match.Matches[0].Groups[1].Value }
    }
}

$yamlMerge = $candidates | Where-Object { $_.Version -eq $wantedVersion } | Select-Object -First 1
if (-not $yamlMerge) {
    $yamlMerge = $candidates | Sort-Object { Get-UnityVersionKey $_.Version } -Descending | Select-Object -First 1
}

if ($yamlMerge) {
    $path = $yamlMerge.Path -replace '\\', '/'
    git config --global merge.unityyamlmerge.name 'Unity SmartMerge'
    git config --global merge.unityyamlmerge.driver "'$path' merge -p %O %B %A %A"
    git config --global merge.unityyamlmerge.recursive binary
    Write-Host "[설정] Unity Smart Merge: $path"
} else {
    Write-Host '[건너뜀] UnityYAMLMerge를 찾지 못했습니다. Unity Hub로 에디터를 설치한 뒤 다시 실행하세요.'
    Write-Host '        Hub를 쓰지 않고 설치했다면 .gitattributes 상단 설명대로 경로를 직접 등록하세요.'
}

# 4. Unity 프로젝트에 pipeline 패키지 추가 (에디터를 열기 전에)
if ($ProjectPath) {
    unity pipeline install --project-path $ProjectPath
    Write-Host "[설정] com.unity.pipeline: $ProjectPath"
}

# 5. GitHub 로그인 확인
gh auth status *> $null
if ($LASTEXITCODE -eq 0) {
    Write-Host '[있음] GitHub 로그인'
} else {
    Write-Host '[필요] GitHub 로그인이 필요합니다. 직접 실행하세요: gh auth login'
}

Write-Host '완료'
