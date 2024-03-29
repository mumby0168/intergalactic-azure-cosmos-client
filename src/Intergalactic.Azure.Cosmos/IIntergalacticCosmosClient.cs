// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Intergalactic.Azure.Cosmos;

public interface IIntergalacticCosmosClient :
    IReadonlyIntergalacticCosmosClient,
    IWriteOnlyIntergalacticCosmosClient,
    IBatchIntergalacticCosmosClient
{

}