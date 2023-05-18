# TODO Options
# * Add function where we add the configuration files with the publish.
# * Add functioality to Refresh the directroy that we are publishing in.
# * Make a quick publish so we only copy the new files that are needed and tyhe core application and not the libraries that are needed to run.

$PublishDirectory = "D:\Documents\GitHub\Borealis\Borealis\src\Borealis.Drivers.RaspberryPi.Sharp\bin\Release\net7.0\publish\linux-arm\"
Set-Location $PublishDirectory

# Getting the files that we want to publish.
$PublishFiles = Get-ChildItem $PublishDirectory\* -Recurse

# Filter by extension
$excluded = @("*.json", "*.preference")

foreach($ex in $excluded) {
$PublishFiles = $PublishFiles | ? {$_.extension -notlike $ex}
}


# SFTP Initialization settings.
$FTPUsername = "pi"
$FTPPwd = "raspberry"
$Password = ConvertTo-SecureString $FTPPwd -AsPlainText -Force
$Credential = New-Object System.Management.Automation.PSCredential ($FTPUsername, $Password)

# Device Settings
$DriverHomeLocation = "/home/pi"
$DriverPath = "/home/pi/borealis/driver"

# Starting the connection
Write-Output "Starting connection to the SFTP host"
$SFTPSession = New-SFTPSession -ComputerName 10.0.2.180 -Credential $Credential

Set-SFTPLocation -SFTPSession $SFTPSession -Path $DriverHomeLocation
Test-SFTPPath -SFTPSession $SFTPSession -Path $DriverHomeLocation

Get-SFTPLocation -SFTPSession $SFTPSession 

write-output "start sending all the files"
foreach($file in $publishfiles) {
    write-output $"copying file $file to $driverpath"
    set-sftpitem -sftpsession $sftpsession -destination $DriverPath -path $file -force
}

write-output "all files send"
