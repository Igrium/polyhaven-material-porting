package com.igrium.metadata_server.asset;

import com.google.gson.annotations.SerializedName;

public record AssetMeta(
        @SerializedName("AssetEntry") AssetEntry assetEntry,
        @SerializedName("poly_id") String polyID,
        @SerializedName("asset_party_url") String assetPartyURL) {

    public String toString() {
        return assetEntry.name();
    }
}
