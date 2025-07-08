package com.example.chatapp

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext
import okhttp3.MediaType.Companion.toMediaType
import okhttp3.OkHttpClient
import okhttp3.Request
import okhttp3.RequestBody.Companion.toRequestBody
import org.json.JSONObject
import androidx.compose.runtime.rememberCoroutineScope
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.DisposableEffect
import com.example.chatapp.BuildConfig

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContent { ChatApp() }
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ChatApp() {
    var messages by remember { mutableStateOf(listOf<String>()) }
    var userInput by remember { mutableStateOf("") }
    // get a CoroutineScope tied to this composable
    val scope = rememberCoroutineScope()

    Column(modifier = Modifier.padding(16.dp)) {
        messages.forEach { Text(it) }
        Spacer(Modifier.weight(1f))
        OutlinedTextField(
            value = userInput,
            onValueChange = { userInput = it },
            modifier = Modifier.fillMaxWidth()
        )
        Button(
            onClick = {
                val input = userInput
                userInput = ""
                messages = messages + "Me: $input"

                // launch the network call on Dispatchers.IO
                scope.launch {
                    val reply = sendMessage(input)
                    messages = messages + "Bot: $reply"
                }
            },
            modifier = Modifier
                .fillMaxWidth()
                .padding(top = 8.dp)
        ) {
            Text("Send")
        }
    }
}

suspend fun sendMessage(text: String): String = withContext(Dispatchers.IO) {
    // now BuildConfig is in scope
    val key = BuildConfig.OPENAI_API_KEY

    val json = JSONObject().apply {
        put("model", "gpt-3.5-turbo")
        put("messages", listOf(mapOf("role" to "user", "content" to text)))
    }

    val client = OkHttpClient()
    val mediaType = "application/json".toMediaType()
    val body = json.toString().toRequestBody(mediaType)

    val request = Request.Builder()
        .url("https://api.openai.com/v1/chat/completions")
        .header("Authorization", "Bearer $key")
        .post(body)
        .build()

    client.newCall(request).execute().use { resp ->
        if (!resp.isSuccessful) return@withContext "Error ${resp.code}"
        JSONObject(resp.body?.string() ?: "").run {
            getJSONArray("choices")
                .getJSONObject(0)
                .getJSONObject("message")
                .getString("content")
        }
    }
}
