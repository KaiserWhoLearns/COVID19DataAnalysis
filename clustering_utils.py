from sklearn.metrics import silhouette_score, calinski_harabasz_score
import numpy as np

def get_score(data, labels, score_type = "silhouette"):
    if score_type == "silhouette":
        return silhouette_score(data, labels)
    else:
        return calinski_harabasz_score(data, labels)
    
def choose_best_score(all_data, all_labels, score_type = "silhouette"):
    all_scores = []
    for data, labels in zip(all_data, all_labels):
        all_scores.append(get_score(data, labels, score_type))
    best_idx = np.argmax(all_scores)
    return all_data[best_idx], all_labels[best_idx]
