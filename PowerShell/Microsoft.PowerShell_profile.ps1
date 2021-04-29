function prompt() {
    # カレント・フォルダのパスから末尾の要素のみを抽出
    "[" + (Split-Path (Get-Location) -Leaf) + "] > "
}

# AutomationConnectIQ.Utilityのdllをローディング
Get-ChildItem -Filter *.dll -Recurse (Split-Path (Get-Package AutomationConnectIQ.Utility).Source) |
  ForEach-Object { Add-Type -LiteralPath $_ }