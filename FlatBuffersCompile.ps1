Write-Output "Flat Buffers Compiler Script."

# Functions
Write-Output "Running functions."
function Create-ArgumentList {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true)]
        [string[]]$Strings
    )

    $arguments = ""
    foreach ($s in $Strings) {
        $arguments += "$s "
    }

    return $arguments.TrimEnd()
}


#Scrips
Write-Output "Running core script."
$FlatBuffersLocation = "D:\Documents\Tools\FlatBuffers\flatc.exe"
$FlatBufferSchema = "D:\Documents\GitHub\Borealis\Borealis\communication\MicrocontrollerMessages.fbs"


# ---------- C# ----------- Portal ------------
Write-Output "Running C# Compiler."
$CSharpOutputDirectory = "D:\Documents\GitHub\Borealis\Borealis\src\Borealis.Portal.Infrastructure\Messages"

# Setting output directory
# Setting the directory because the flatc compiler will compile the files into the current running directory.
Set-Location $CSharpOutputDirectory
Write-Output "Setting current and ouput directory: $CSharpOutputDirectory"

#ArgumentList
$CSharpArguments = Create-ArgumentList -Strings  "--csharp", "--gen-object-api", "--gen-onefile", $FlatBufferSchema
$CSharpGeneratedCMDCommand = "$FlatBuffersLocation $CSharpArguments"
Write-Output "Generated command :" $CSharpGeneratedCMDCommand

# Running the compiler.
Write-Output "Running output flat compiler!"
cmd.exe /c $CSharpGeneratedCMDCommand

Write-Output "Portal Compiler done running."


# ------ Drivers -------------


# ----------- Raspberry Pi Sharp --------------------
Write-Output "Running Raspberry Pi Sharp Compiler"
$RaspberryPiSharpOutputDirectory = "D:\Documents\GitHub\Borealis\Borealis\src\Borealis.Drivers.RaspberryPi.Sharp\Communication\Messages"


Set-Location $RaspberryPiSharpOutputDirectory
Write-Output "Setting current and ouput directory: $CSharpOutputDirectory"

$RaspberryPiSharpArguments = Create-ArgumentList -Strings  "--csharp", "--gen-object-api", "--gen-onefile", $FlatBufferSchema
$RaspberryPiSharpGeneratedCMDCommand = "$FlatBuffersLocation $RaspberryPiSharpArguments"
Write-Output "Generated command :" $RaspberryPiSharpGeneratedCMDCommand

Write-Output "Running output flat compiler!"
cmd.exe /c $RaspberryPiSharpGeneratedCMDCommand

Write-Output "Raspberry Pi Sharp Compiler done running."

# ----------------------------

