package com.igrium.metadata_server.screenshots;

import java.io.BufferedInputStream;
import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.net.MalformedURLException;
import java.net.URL;
import java.util.concurrent.CompletableFuture;
import java.util.concurrent.ForkJoinPool;

public class ScreenshotManager {

    public static CompletableFuture<File> downloadScreenshot(String id) {
        File dest = new File("screenshots/"+id+".png");
        System.out.println("Saving screenshot for " + id + " to " + dest.getAbsolutePath());
        URL url;
        try {
            url = new URL("https://cdn.polyhaven.com/asset_img/thumbs/" + id + ".png");
        } catch (MalformedURLException e) {
            throw new RuntimeException(e);
        }
        return downloadFileAsync(url, dest).thenApply((v) -> dest);
    }

    public static void downloadFile(URL url, File dest) throws IOException {
        dest.getParentFile().mkdirs();
        try (BufferedInputStream in = new BufferedInputStream(url.openStream());
                FileOutputStream out = new FileOutputStream(dest)) {
            byte[] buffer = new byte[1024];
            int bytesRead;

            while ((bytesRead = in.read(buffer, 0, 1024)) != -1) {
                out.write(buffer, 0, bytesRead);
            }
        }
    }

    public static CompletableFuture<Void> downloadFileAsync(URL url, File dest) {
        CompletableFuture<Void> future = new CompletableFuture<>();
        ForkJoinPool.commonPool().submit(() -> {
            try {
                downloadFile(url, dest);
            } catch (Exception e) {
                future.completeExceptionally(e);
            }
            future.complete(null);
        });

        return future;
    }
}
