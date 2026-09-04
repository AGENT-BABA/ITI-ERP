# Fetches comprehensive Indian PIN code data and transforms it for the ERP system
# Source: bilal-webdev/india-postal-pincode-dataset (MIT License)

$sourceUrl = "https://raw.githubusercontent.com/bilal-webdev/india-postal-pincode-dataset/main/json/india-postal-by-pincode.json"
$outputPath = "C:\Users\Krushna\Desktop\ITI\src\Application\ITI.ERP.Application\Location\IndiaPinCodes.json"

Write-Host "Downloading Indian PIN code dataset..."
$tempFile = "$env:TEMP\india-pincode-raw.json"
Invoke-WebRequest -Uri $sourceUrl -OutFile $tempFile -UseBasicParsing

Write-Host "Transforming data..."
$raw = Get-Content $tempFile -Raw | ConvertFrom-Json

# Transform to our format: { "PIN": [ { name, district, state, taluk, division } ] }
$result = @{}
foreach ($pin in $raw.PSObject.Properties) {
    $entries = @()
    foreach ($district in $pin.Value.districts) {
        foreach ($block in $district.blocks) {
            $entries += @{
                name = $block
                district = $district.district
                state = $pin.Value.state
                taluk = $null
                division = $null
            }
        }
    }
    if ($entries.Count -gt 0) {
        $result[$pin.Name] = $entries
    }
}

# Write formatted JSON
$result | ConvertTo-Json -Depth 10 | Set-Content $outputPath -Encoding UTF8
Write-Host "Done! Wrote $($result.Count) PIN codes to $outputPath"

# Cleanup
Remove-Item $tempFile
