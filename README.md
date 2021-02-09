# cardano-rbac
Role-based Access Control Implemented on the Cardano Blockchain

We're designing a format specification (T-RBAC) and a .NET Core (and .NET 5) class library to assist in implementing that protocol.

## T-RBAC
Transactional Role-based Access Control (T-RBAC) is a format specification we're designing to facilitate the storage of RBAC policies on UTXO-based blockchains like Cardano. See the latest draft here: https://docs.google.com/document/d/e/2PACX-1vR0a1tpsND8JbUjFI9jTx4uls3ivtL0odYHeQKwZbJ8LhqLEAQP4hSRzuqlfvXmFc8ihbxbqxwgBslB/pub

### JSON Schemas
The T-RBAC JSON schemas are held in a separate "schemas" repository: https://github.com/fullcircl/schemas/tree/main/rbac/draft

## CardanoRbac
CardanoRbac is a .NET Standard 2.0 portable class library for interacting with RBAC policy metadata on the blockchain.

## CardanoRbac.Cli
CardanoRbac.Cli is a command-line executable program for validating and querying RBAC policy metadata.

## Status
cardano-rbac is currently in early development stages. The schemas, class library, and CLI are all in early alpha stage, and are not suitable for use on a mainnet.

## Pre-requisites
[.NET 5 SDK]

## Development environment

We recommend Visual Studio 2019 for easy debugging.

Clone this repo, git clone https://github.com/fullcircl/cardano-rbac.git
