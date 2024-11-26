# Define branches to keep
$protectedBranches = @("main", "develop")

# Get all local branches
$branches = git branch | ForEach-Object { $_.Trim() }

# Identify the current branch (marked with *)
$currentBranch = ($branches | Where-Object { $_ -like "* *" }) -replace "\* ", ""

# Add the current branch to the list of protected branches
$protectedBranches += $currentBranch

# Delete all branches except the protected ones
$branches | ForEach-Object {
    $branchName = $_ -replace "\* ", ""  # Remove leading '* ' from branch name
    if (-not $protectedBranches.Contains($branchName)) {
        git branch -D $branchName
        Write-Host "Deleted branch: $branchName"
    }
}

Write-Host "Protected branches: $protectedBranches"
Write-Host "All other branches have been deleted."
