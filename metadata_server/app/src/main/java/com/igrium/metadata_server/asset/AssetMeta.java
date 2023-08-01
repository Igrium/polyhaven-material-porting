package com.igrium.metadata_server.asset;

import com.google.gson.annotations.SerializedName;

public record AssetMeta(
        @SerializedName("AssetEntry") AssetEntry assetEntry,
        @SerializedName("poly_id") String polyID) {

    public String toString() {
        return assetEntry.name();
    }
}
