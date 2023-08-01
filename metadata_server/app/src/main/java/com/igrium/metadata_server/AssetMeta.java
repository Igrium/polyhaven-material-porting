package com.igrium.metadata_server;

import com.google.gson.annotations.SerializedName;

public record AssetMeta(
        @SerializedName("AssetEntry") AssetEntry assetEntry,

        @SerializedName("poly_id") String polyID) {
}
