import urllib.request
import urllib.parse
import json
import xml.etree.ElementTree as ET

BASE_URL = "http://localhost:3000"

def test_frontend_html():
    print("1. Testing Frontend HTML...")
    req = urllib.request.Request(BASE_URL)
    with urllib.request.urlopen(req) as resp:
        content = resp.read().decode('utf-8')
        assert resp.status == 200, f"Expected 200, got {resp.status}"
        assert "Excel to XML Converter" in content, "Missing title in HTML"
        print("   ✅ Frontend served index.html successfully.")

def test_download_templates():
    print("2. Testing Template Downloads...")
    # SOT Production
    prod_url = f"{BASE_URL}/api/converter/template/sot-production"
    req_prod = urllib.request.Request(prod_url)
    with urllib.request.urlopen(req_prod) as resp:
        prod_bytes = resp.read()
        assert resp.status == 200, "Failed to download SOT Production template"
        assert len(prod_bytes) > 1000, "Template bytes too small"
        with open("template_sot_production.xlsx", "wb") as f:
            f.write(prod_bytes)
        print(f"   ✅ SOT Production template downloaded ({len(prod_bytes)} bytes).")

    # SOT L-PUMA
    lpuma_url = f"{BASE_URL}/api/converter/template/sot-lpuma"
    req_lpuma = urllib.request.Request(lpuma_url)
    with urllib.request.urlopen(req_lpuma) as resp:
        lpuma_bytes = resp.read()
        assert resp.status == 200, "Failed to download SOT L-PUMA template"
        assert len(lpuma_bytes) > 1000, "Template bytes too small"
        with open("template_sot_lpuma.xlsx", "wb") as f:
            f.write(lpuma_bytes)
        print(f"   ✅ SOT L-PUMA template downloaded ({len(lpuma_bytes)} bytes).")

def post_multipart(url, fields, files):
    boundary = "----WebKitFormBoundary7MA4YWxkTrZu0gW"
    data = []
    for k, v in fields.items():
        data.append(f"--{boundary}\r\n".encode('utf-8'))
        data.append(f'Content-Disposition: form-data; name="{k}"\r\n\r\n'.encode('utf-8'))
        data.append(f"{v}\r\n".encode('utf-8'))
    for k, (filename, file_bytes) in files.items():
        data.append(f"--{boundary}\r\n".encode('utf-8'))
        data.append(f'Content-Disposition: form-data; name="{k}"; filename="{filename}"\r\n'.encode('utf-8'))
        data.append(b"Content-Type: application/vnd.openxmlformats-officedocument.spreadsheetml.sheet\r\n\r\n")
        data.append(file_bytes)
        data.append(b"\r\n")
    data.append(f"--{boundary}--\r\n".encode('utf-8'))
    body = b"".join(data)

    req = urllib.request.Request(url, data=body)
    req.add_header("Content-Type", f"multipart/form-data; boundary={boundary}")
    with urllib.request.urlopen(req) as resp:
        return resp.status, json.loads(resp.read().decode('utf-8'))

def test_convert_sot_production():
    print("3. Testing SOT Production Conversion...")
    with open("template_sot_production.xlsx", "rb") as f:
        file_bytes = f.read()

    status, result = post_multipart(
        f"{BASE_URL}/api/converter/convert",
        fields={"schemaType": "SOT_PRODUCTION", "customFileName": "SOT_PROD_TEST_20261012"},
        files={"file": ("test_sot_prod.xlsx", file_bytes)}
    )
    assert status == 200, f"Expected 200, got {status}"
    assert result["status"] == "Berhasil", f"Expected Berhasil, got {result.get('status')}"
    assert result["rowCount"] >= 2, f"Expected at least 2 rows, got {result['rowCount']}"
    
    # Parse XML
    xml_str = result["xmlContent"]
    assert "<productVolumes" in xml_str, "Missing <productVolumes>"
    assert "<productVolume>" in xml_str, "Missing <productVolume>"
    assert "<facility" in xml_str, "Missing <facility>"
    assert "<flow>" in xml_str, "Missing <flow>"
    assert "<product>" in xml_str, "Missing <product>"
    assert "<period>" in xml_str, "Missing <period>"
    assert "<balanceSet>" in xml_str, "Missing <balanceSet>"
    assert "<balanceDetail>" in xml_str, "Missing <balanceDetail>"
    
    print(f"   ✅ SOT Production converted to valid ProdML XML ({result['rowCount']} rows). ID: {result['id']}")
    return result["id"]

def test_convert_sot_lpuma():
    print("4. Testing SOT L-PUMA Conversion...")
    with open("template_sot_lpuma.xlsx", "rb") as f:
        file_bytes = f.read()

    status, result = post_multipart(
        f"{BASE_URL}/api/converter/convert",
        fields={"schemaType": "SOT_LPUMA", "customFileName": "SOT_LPUMA_TEST_20261012"},
        files={"file": ("test_sot_lpuma.xlsx", file_bytes)}
    )
    assert status == 200, f"Expected 200, got {status}"
    assert result["status"] == "Berhasil", f"Expected Berhasil, got {result.get('status')}"
    assert result["rowCount"] >= 2, f"Expected at least 2 rows, got {result['rowCount']}"
    
    # Parse XML
    xml_str = result["xmlContent"]
    assert "<lossProductionOpportunity>" in xml_str, "Missing <lossProductionOpportunity>"
    assert "<documentName>" in xml_str, "Missing <documentName>"
    assert "<events>" in xml_str, "Missing <events>"
    assert "<eventDate" in xml_str, "Missing <eventDate>"
    assert "<facility>" in xml_str, "Missing <facility>"
    assert "<shutdownID>" in xml_str, "Missing <shutdownID>"
    assert "<volumeOil>" in xml_str, "Missing <volumeOil>"
    
    print(f"   ✅ SOT L-PUMA converted to valid XML ({result['rowCount']} rows). ID: {result['id']}")
    return result["id"]

def test_history_and_download(prod_id, lpuma_id):
    print("5. Testing History and XML Download...")
    req = urllib.request.Request(f"{BASE_URL}/api/converter/history")
    with urllib.request.urlopen(req) as resp:
        history = json.loads(resp.read().decode('utf-8'))
        assert history["totalCount"] >= 2, f"Expected >= 2 items, got {history['totalCount']}"
        print(f"   ✅ History returned {history['totalCount']} total records.")

    # Test downloading XML
    req_dl = urllib.request.Request(f"{BASE_URL}/api/converter/download/{prod_id}")
    with urllib.request.urlopen(req_dl) as resp:
        xml_bytes = resp.read()
        assert resp.status == 200
        assert b"<productVolumes" in xml_bytes
        print(f"   ✅ XML download verified for ID {prod_id} ({len(xml_bytes)} bytes).")

if __name__ == "__main__":
    test_frontend_html()
    test_download_templates()
    prod_id = test_convert_sot_production()
    lpuma_id = test_convert_sot_lpuma()
    test_history_and_download(prod_id, lpuma_id)
    print("\n🎉 ALL END-TO-END TESTS PASSED!")
