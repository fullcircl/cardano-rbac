# cardano-rbac
Role-based Access Control Implemented on the Cardano Blockchain

We're designing a protocol and a .NET Core (and .NET 5) class library to assist in implementing that protocol.

## CardanoRbac
CardanoRbac is a .NET Standard 2.0 portable class library for interacting with RBAC policy metadata on the blockchain.

## CardanoRbac.Cli
CardanoRbac.Cli is a command-line executable program for validating and querying RBAC policy metadata.

## rbac JSON Schemas
The 'rbac' JSON schemas define the standard schema for deploying RBAC policies to the blockchain and transacting on them. These are held in a separate "schemas" repository: https://github.com/torus-online/schemas/tree/main/rbac/draft

## Status
cardano-rbac is currently in early development stages. The schemas, class library, and CLI are all in early alpha stage, and are not suitable for use on a mainnet.

## Pre-requisites
[.NET 5 SDK]

## Development environment

We recommend Visual Studio 2019 for easy debugging.

Clone this repo, git clone https://github.com/torus-online/cardano-rbac.git
