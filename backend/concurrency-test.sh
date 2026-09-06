#!/usr/bin/env bash
# Envía dos pujas idénticas en simultáneo sobre la subasta Id 1 (activa estándar,
# líder actual comprador1 con $45.000, VersionEsperada = 2 según el seed data)
# para demostrar que la base registra solo una y rechaza la otra con 409 Conflict.

API_URL="${1:-http://localhost:5000}"
SUBASTA_ID=1
BODY='{"CompradorId":3,"Monto":46000,"VersionEsperada":2}'

echo "Disparando 2 pujas idénticas en paralelo contra $API_URL/api/v1/auctions/$SUBASTA_ID/bids ..."

curl -s -o resp1.json -w "Request 1 -> HTTP %{http_code}\n" \
  -X POST "$API_URL/api/v1/auctions/$SUBASTA_ID/bids" \
  -H "Content-Type: application/json" -d "$BODY" &

curl -s -o resp2.json -w "Request 2 -> HTTP %{http_code}\n" \
  -X POST "$API_URL/api/v1/auctions/$SUBASTA_ID/bids" \
  -H "Content-Type: application/json" -d "$BODY" &

wait

echo ""
echo "--- Respuesta 1 ---"
cat resp1.json
echo ""
echo "--- Respuesta 2 ---"
cat resp2.json
echo ""
echo "Se espera un 200 OK y un 409 Conflict."

rm -f resp1.json resp2.json
