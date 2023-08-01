package com.igrium.metadata_server.ui;

import com.igrium.metadata_server.asset.AssetMeta;

import javafx.beans.value.ChangeListener;
import javafx.beans.value.ObservableValue;
import javafx.collections.ObservableList;
import javafx.fxml.FXML;
import javafx.scene.control.ListView;
import javafx.scene.layout.BorderPane;

public class MainWindow {

    @FXML
    private BorderPane rootPane;

    @FXML
    private ListView<AssetMeta> queue;

    public BorderPane getRootPane() {
        return rootPane;
    }

    public ListView<AssetMeta> getQueue() {
        return queue;
    }

    public ObservableList<AssetMeta> queueItems() {
        return queue.getItems();
    }

    @FXML
    protected void initialize() {
        queue.getSelectionModel().selectedItemProperty().addListener(new ChangeListener<>() {

            @Override
            public void changed(ObservableValue<? extends AssetMeta> observable, AssetMeta oldValue,
                    AssetMeta newValue) {
                if (newValue == null) {
                    rootPane.setBottom(null);
                    return;
                }

                var uiPair = MetaUI.create();
                uiPair.getKey().Load(newValue);
                rootPane.setBottom(uiPair.getValue());

                uiPair.getKey().getDoneButton().setOnMouseClicked(e -> {
                    queue.getItems().remove(newValue);
                });
            }

        });
    }

}
