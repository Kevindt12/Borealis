Write-Output "Protobuf Message Compiler."

# Functions
Write-Output "Registering local functions."
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
$ProtobufCompilerPath = "D:\Documents\Tools\Protobuf\bin\protoc.exe"
$ProtobufSchema = "D:\Documents\GitHub\Borealis\Borealis\src\Borealis.Networking\Messages\Protos\Messages.proto"
$ProtobufPath = "D:\Documents\GitHub\Borealis\Borealis\src\Borealis.Networking\Messages\Protos"
$ProtobufImports = "D:\Documents\Tools\Protobuf\include\"




# ---------- C# ----------- Portal ------------
Write-Output "Running C# Compiler."
$CSharpOutputDirectory = "D:\Documents\GitHub\Borealis\Borealis\src\Borealis.Networking\Messages"

# Setting output directory
# Setting the directory because the flatc compiler will compile the files into the current running directory.
Write-Output "Setting current and ouput directory: $CSharpOutputDirectory"

#ArgumentList
$CSharpArguments = Create-ArgumentList -Strings "--proto_path=$ProtobufImports","--proto_path=$ProtobufPath", "--csharp_out=$CSharpOutputDirectory", $ProtobufSchema
$CSharpGeneratedCMDCommand = "$ProtobufCompilerPath $CSharpArguments"
Write-Output "Generated command :" $CSharpGeneratedCMDCommand

# Running the compiler.
Write-Output "Running output flat compiler!"
cmd.exe /c $CSharpGeneratedCMDCommand

Write-Output "Protobuf compiler done."

