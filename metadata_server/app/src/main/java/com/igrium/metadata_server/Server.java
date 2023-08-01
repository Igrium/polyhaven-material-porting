package com.igrium.metadata_server;

import com.google.gson.Gson;
import com.google.gson.GsonBuilder;

import io.javalin.Javalin;
import io.javalin.json.JsonMapper;

public class Server {
    private Javalin app;
    private Gson gson = new GsonBuilder().create();
    private JsonMapper gsonMapper = new JsonMapper() {

        public String toJsonString(Object obj, java.lang.reflect.Type type) {
            return gson.toJson(obj, type);
        };

        public <T> T fromJsonString(String json, java.lang.reflect.Type targetType) {
            return gson.fromJson(json, targetType);
        };
    };

    public Javalin getApp() {
        return app;
    }

    public void start(int port) {
        app = Javalin.create(config -> {
            config.jsonMapper(gsonMapper);
        });
        app.post("/submit", ctx -> {
            onSubmit(ctx.bodyAsClass(AssetMeta.class));
        });

        app.start();
        System.out.println("Server started on port " + app.port());
    }

    protected void onSubmit(AssetMeta meta) {
        
    }

}
