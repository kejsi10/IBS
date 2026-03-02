#!/usr/bin/env powershell
<#
.SYNOPSIS
    Pulls the required Ollama AI models for IBS document features.

.DESCRIPTION
    Run once after starting the Docker Compose stack:
    docker-compose -f docker/docker-compose.dev.yml up -d
#>

$ErrorActionPreference = "Stop"
$Container = "ibs-ollama"

Write-Host "Waiting for Ollama container to be ready..." -ForegroundColor Cyan
$ready = $false
for ($i = 0; $i -lt 30; $i++) {
    $result = docker exec $Container ollama list 2>&1
    if ($LASTEXITCODE -eq 0) { $ready = $true; break }
    Start-Sleep -Seconds 2
}

if (-not $ready) {
    Write-Error "Ollama container '$Container' is not responding. Is the stack running?`ndocker-compose -f docker/docker-compose.dev.yml up -d"
    exit 1
}

Write-Host "Pulling qwen2.5-coder:7b (code model for template editing and PDF import)..." -ForegroundColor Cyan
docker exec $Container ollama pull qwen2.5-coder:7b
if ($LASTEXITCODE -ne 0) { Write-Error "Failed to pull qwen2.5-coder:7b"; exit 1 }

Write-Host "Pulling llama3.1:8b (chat model for policy assistant)..." -ForegroundColor Cyan
docker exec $Container ollama pull llama3.1:8b
if ($LASTEXITCODE -ne 0) { Write-Error "Failed to pull llama3.1:8b"; exit 1 }

Write-Host "All models ready. AI document features are available." -ForegroundColor Green
