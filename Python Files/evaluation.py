import pandas as pd
from scipy import stats
import matplotlib.pyplot as plt

df = pd.read_csv("responses.csv")

adaptive_cols = [
    "I felt immersed in the virtual environment. ",
    "I felt physically present within the virtual space. ",
    "I lost awareness of the real world during the interaction. ",
    "The experience felt consistent and believable. ",
    "I felt emotionally involved in the interaction. "
]

control_cols = [
    "I felt immersed in the virtual environment. .1",
    "I felt physically present within the virtual space. .1",
    "I lost awareness of the real world during the interaction. .1",
    "The experience felt consistent and believable. .1",
    "I felt emotionally involved in the interaction. .1"
]

# Convert to numeric
for col in adaptive_cols + control_cols:
    df[col] = pd.to_numeric(df[col], errors='coerce')

adaptive_mean = df[adaptive_cols].mean(axis=1)
control_mean = df[control_cols].mean(axis=1)

# Paired t-test
t_stat, p_val = stats.ttest_rel(adaptive_mean, control_mean)

# Effect size
diff = adaptive_mean - control_mean
cohens_d = diff.mean() / diff.std(ddof=1)

print(t_stat, p_val, cohens_d)

# Compute overall means
adaptive_overall_mean = adaptive_mean.mean()
control_overall_mean = control_mean.mean()

# Bar Chart

plt.figure()
plt.bar(["Adaptive", "Control"], [adaptive_overall_mean, control_overall_mean])
plt.ylabel("Mean Immersion Score")
plt.title("Comparison of Immersion Scores (Adaptive vs Control)")
plt.savefig("bar_chart_means.png")
plt.show()

# Box Plot

plt.figure()
plt.boxplot([adaptive_mean, control_mean], labels=["Adaptive", "Control"])
plt.ylabel("Immersion Score")
plt.title("Distribution of Immersion Scores")
plt.savefig("boxplot_distribution.png")
plt.show()

# Paired Line Plot

plt.figure()

for i in range(len(adaptive_mean)):
    plt.plot([1, 2], [adaptive_mean[i], control_mean[i]], marker='o')

plt.xticks([1, 2], ["Adaptive", "Control"])
plt.ylabel("Immersion Score")
plt.title("Participant-Level Comparison (Paired Design)")
plt.savefig("paired_plot.png")
plt.show()