package com.igrium.metadata_server.ui;

import javafx.collections.ObservableList;
import javafx.fxml.FXML;
import javafx.scene.control.ListView;
import javafx.scene.layout.BorderPane;

public class MainWindow {

    @FXML
    private BorderPane rootPane;

    @FXML
    private ListView<String> queue;

    public BorderPane getRootPane() {
        return rootPane;
    }

    public ListView<String> getQueue() {
        return queue;
    }

    public ObservableList<String> queueItems() {
        return queue.getItems();
    }
}
