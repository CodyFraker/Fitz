#!/bin/bash

# Find all .razor files and update the using directives
find . -name "*.razor" -type f -exec sed -i 's/@using Fitz.Shared.Models/@using Fitz.Features.Accounts.Models\n@using Fitz.Features.Lottery.Models/g' {} + 