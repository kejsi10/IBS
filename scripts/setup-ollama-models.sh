#!/usr/bin/env bash
# setup-ollama-models.sh
# Pulls the required Ollama AI models for IBS document features.
# Run once after starting the Docker Compose stack (docker-compose -f docker/docker-compose.dev.yml up -d).

set -e

CONTAINER="ibs-ollama"

echo "Waiting for Ollama container to be ready..."
until docker exec "$CONTAINER" ollama list >/dev/null 2>&1; do
    sleep 2
done

echo "Pulling qwen2.5:3b (model for template editing, PDF import, and policy assistant chat)..."
docker exec "$CONTAINER" ollama pull qwen2.5:3b

echo "All models ready. AI document features are available."
